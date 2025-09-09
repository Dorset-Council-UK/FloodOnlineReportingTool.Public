using FloodOnlineReportingTool.Database.DbContexts;
using FloodOnlineReportingTool.Database.Models;
using FloodOnlineReportingTool.Database.Settings;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace FloodOnlineReportingTool.Database.Repositories;

public class FloodReportRepository(
    ILogger<FloodReportRepository> logger,
    PublicDbContext context,
    ICommonRepository commonRepository,
    IPublishEndpoint publishEndpoint,
    IOptions<GISSettings> options
) : IFloodReportRepository
{
    private readonly GISSettings _gisSettings = options.Value;

    public async Task<FloodReport?> ReportedByUser(Guid userId, CancellationToken ct)
    {
        return await context.FloodReports
            .AsNoTracking()
            .AsSplitQuery()
            .Include(o => o.EligibilityCheck)
            .Include(o => o.Investigation)
            .Include(o => o.ContactRecords)
            .Include(o => o.Status)
            .FirstOrDefaultAsync(o => o.ReportedByUserId == userId, ct)
            .ConfigureAwait(false);
    }

    private string CreateReference()
    {
        var reference = Guid.CreateVersion7().ToString("N")[..8].ToUpperInvariant();
        logger.LogInformation("Creating a new flood report reference number {Reference}.", reference);
        return reference;
    }

    public async Task<FloodReport?> GetByReference(string reference, CancellationToken ct)
    {
        logger.LogInformation("Getting flood report by reference number {Reference}.", reference);

        // Include all related tables
        return await context.FloodReports
            .AsNoTracking()
            .AsSplitQuery()
            .Include(o => o.EligibilityCheck)
            .Include(o => o.Investigation)
            .Include(o => o.ContactRecords)
            .Include(o => o.Status)
            .FirstOrDefaultAsync(o => o.Reference == reference, ct)
            .ConfigureAwait(false);
    }

    public async Task<(bool hasFloodReport, bool hasInvestigation, bool hasInvestigationStarted, DateTimeOffset? investigationCreatedUtc)> ReportedByUserBasicInformation(Guid userId, CancellationToken ct)
    {
        logger.LogInformation("Getting flood report details by user {UserId}.", userId);

        // In simple terms only 2 fields are needed, StatusId and Investigation.CreatedUtc
        // Calling the standard ReportedByUser method is not efficient as it loads all related tables

        var result = await context.FloodReports
            .AsNoTracking()
            .Where(o => o.ReportedByUserId == userId)
            .Select(o => new
            {
                o.StatusId,
                o.Investigation,
            })
            .FirstOrDefaultAsync(ct)
            .ConfigureAwait(false);

        if (result == null)
        {
            logger.LogWarning("No flood report found for user {UserId}.", userId);
            return (false, false, false, null);
        }

        var hasInvestigation = false;
        DateTimeOffset? investigationCreatedUtc = null;
        if (result.Investigation != null)
        {
            hasInvestigation = true;
            investigationCreatedUtc = result.Investigation.CreatedUtc;
        }
        return (true, hasInvestigation, HasInvestigationStarted(result.StatusId), investigationCreatedUtc);
    }

    public async Task<FloodReport> Create(CancellationToken ct)
    {
        logger.LogInformation("Creating a new flood report.");

        var now = DateTimeOffset.UtcNow;

        var floodReport = new FloodReport
        {
            Reference = CreateReference(),
            CreatedUtc = now,
            StatusId = RecordStatusIds.New,
            UserAccessUntilUtc = now.AddMonths(_gisSettings.AccessTokenIssueDurationMonths),
        };

        context.FloodReports.Add(floodReport);

        // Publish a created message to the message system?
        var message = floodReport.ToMessageCreated();
        await publishEndpoint
            .Publish(message, ct)
            .ConfigureAwait(false);

        // Add both the flood report and the message to the database
        await context
            .SaveChangesAsync(ct)
            .ConfigureAwait(false);

        return floodReport;
    }

    public async Task<FloodReport> CreateWithEligiblityCheck(EligibilityCheckDto dto, CancellationToken ct)
    {
        logger.LogInformation("Creating a new flood report with eligibility check.");

        var eligibilityCheckId = Guid.CreateVersion7();
        var now = DateTimeOffset.UtcNow;

        var impactDuration = await GetImpactDurationHours(dto.OnGoing, dto.DurationKnownId, dto.ImpactDuration, ct).ConfigureAwait(false);
        
        var floodReport = new FloodReport
        {
            Reference = CreateReference(),
            CreatedUtc = now,
            StatusId = RecordStatusIds.New,
            UserAccessUntilUtc = now.AddMonths(_gisSettings.AccessTokenIssueDurationMonths),
            EligibilityCheck = new()
            {
                Id = eligibilityCheckId,
                CreatedUtc = now,
                TermsAgreed = now,

                IsAddress = dto.IsAddress,
                Uprn = dto.Uprn,
                Easting = dto.Easting,
                Northing = dto.Northing,
                LocationDesc = dto.LocationDesc,
                ImpactStart = dto.ImpactStart,
                ImpactDuration = impactDuration,
                OnGoing = dto.OnGoing,
                Uninhabitable = dto.Uninhabitable == true,
                VulnerablePeopleId = dto.VulnerablePeopleId,
                VulnerableCount = dto.VulnerableCount,
                Residentials = [.. dto.Residentials.Select(floodImpactId => new EligibilityCheckResidential(eligibilityCheckId, floodImpactId))],
                Commercials = [.. dto.Commercials.Select(floodImpactId => new EligibilityCheckCommercial(eligibilityCheckId, floodImpactId))],
                Sources = [.. dto.Sources.Select(floodProblemId => new EligibilityCheckSource(eligibilityCheckId, floodProblemId))],
            },
        };

        // Add the flood report to the database
        context.FloodReports.Add(floodReport);

        // Publish mutiple messages to the message system
        var responsibleOrganisations = await commonRepository
            .GetResponsibleOrganisations(floodReport.EligibilityCheck.Easting, floodReport.EligibilityCheck.Northing, ct)
            .ConfigureAwait(false);
        var floodReportCreatedMessage = floodReport.ToMessageCreated();
        IList<FloodProblem> sourcesToFilter = context.FloodProblems.Where(e => floodReport.EligibilityCheck.Sources.Select(s => s.FloodProblemId).ToList().Contains(e.Id)).ToList();
        var floodSources = await commonRepository.FilterFloodProblemsByCategories(
            [FloodProblemCategory.PrimaryCause, FloodProblemCategory.SecondaryCause],
            sourcesToFilter,
            ct).ConfigureAwait(false);
        var eligibilityCheckCreatedMessage = floodReport.EligibilityCheck.ToMessageCreated(floodReport.Reference, responsibleOrganisations, floodSources);

        await publishEndpoint.Publish(floodReportCreatedMessage, ct).ConfigureAwait(false);
        await publishEndpoint.Publish(eligibilityCheckCreatedMessage, ct).ConfigureAwait(false);

        // Save the flood report, eligibility check, and messages to the database
        await context.SaveChangesAsync(ct).ConfigureAwait(false);

        return floodReport;
    }

    public bool HasInvestigationStarted(Guid status)
    {
        return status == RecordStatusIds.ActionNeeded;
    }

    /// <summary>
    ///     <para>Calculates the impact duration hours.</para>
    ///     <para>If the flood is still happening this will be zero.</para>
    ///     <para>If the flood duration is known, it will return the hours provided by the user.</para>
    ///     <para>Otherwise, it will try to get the duration hours from the flood problem in the database.</para>
    /// </summary>
    /// <remarks>This logic is currently in 2 places the FloodReportRepository and the EligibilityCheckRespository.</remarks>
    private async Task<int> GetImpactDurationHours(bool isOngoing, Guid? durationKnownId, int? impactDurationHours, CancellationToken ct)
    {
        // The flood is still happening, so there is no duration
        if (isOngoing)
        {
            logger.LogInformation("Flood is ongoing, so impact duration is not known yet.");
            return 0;
        }

        if (durationKnownId == null)
        {
            logger.LogError("Impact duration is not known, and no duration known Id was provided.");
            return 0;
        }

        // The user has indicated that the flood duration is known
        if (durationKnownId == FloodProblemIds.DurationKnown)
        {
            logger.LogInformation("Impact duration is known, using provided impact duration hours.");
            return impactDurationHours ?? 0;
        }

        // Get the duration from the flood problem
        var floodProblem = await context.FloodProblems
            .FindAsync([durationKnownId], ct)
            .ConfigureAwait(false);

        if (floodProblem == null)
        {
            logger.LogError("Flood problem with Id {Id} not found.", durationKnownId);
            return 0;
        }

        if (!string.IsNullOrWhiteSpace(floodProblem.TypeName) && int.TryParse(floodProblem.TypeName, CultureInfo.InvariantCulture, out var durationHours))
        {
            logger.LogInformation("Impact duration is {Duration} hours.", durationHours);
            return durationHours;
        }

        logger.LogError("Could not parse impact duration from flood problem type name");
        return 0;
    }
}

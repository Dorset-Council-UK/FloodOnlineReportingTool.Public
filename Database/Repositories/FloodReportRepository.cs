using FloodOnlineReportingTool.Contracts.Shared;
using FloodOnlineReportingTool.Database.DbContexts;
using FloodOnlineReportingTool.Database.Models.Eligibility;
using FloodOnlineReportingTool.Database.Models.Flood;
using FloodOnlineReportingTool.Database.Models.Flood.FloodProblemIds;
using FloodOnlineReportingTool.Database.Options;
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
    IOptions<GISOptions> options
) : IFloodReportRepository
{
    private readonly GISOptions _gisSettings = options.Value;

    public async Task<FloodReport?> ReportedByUser(Guid userId, CancellationToken ct)
    {
        return await context.FloodReports
            .AsNoTracking()
            .AsSplitQuery()
            .Include(o => o.EligibilityCheck)
            .Include(o => o.Investigation)
            .Include(o => o.ContactRecords)
            .Include(o => o.Status)
            .FirstOrDefaultAsync(o => o.ReportOwnerId == userId, ct);
    }

    public async Task<FloodReport?> ReportedByContact(Guid contactUserId, Guid floodReportId, CancellationToken ct)
    {

        return await context.FloodReports
            .Where(fr => fr.ReportOwner != null &&
                 fr.ReportOwner.ContactUserId == contactUserId &&
                 fr.Id == floodReportId)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyCollection<FloodReport>> AllReportedByContact(Guid contactUserId, CancellationToken ct)
    {
        return await context.FloodReports
            .Where(fc => fc.ReportOwner != null && fc.ReportOwner.ContactUserId == contactUserId)
            .OrderByDescending(cr => cr.CreatedUtc)
            .ToListAsync(ct);
    }

    private string CreateReference()
    {
        var reference = Guid.CreateVersion7().ToString("N")[..8].ToUpperInvariant();
        logger.LogInformation("Creating a new flood report reference number {Reference}.", reference);
        return reference;
    }

    public async Task<FloodReport?> GetById(Guid reference, CancellationToken ct)
    {
        logger.LogInformation("Getting flood report by id {Reference}.", reference);

        // Include all related tables
        return await context.FloodReports
            .AsNoTracking()
            .AsSplitQuery()
            .Include(o => o.EligibilityCheck)
            .Include(o => o.Investigation)
            .Include(o => o.ContactRecords)
            .Include(o => o.Status)
            .FirstOrDefaultAsync(o => o.Id == reference, ct);
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
            .FirstOrDefaultAsync(o => o.Reference == reference, ct);
    }

    public async Task<(bool hasFloodReport, bool hasInvestigation, bool hasInvestigationStarted, DateTimeOffset? investigationCreatedUtc)> ReportedByUserBasicInformation(Guid userId, CancellationToken ct)
    {
        logger.LogInformation("Getting flood report details by user {UserId}.", userId);

        // In simple terms only 2 fields are needed, StatusId and Investigation.CreatedUtc
        // Calling the standard ReportedByUser method is not efficient as it loads all related tables

        var result = await context.FloodReports
            .AsNoTracking()
            .Where(o => o.ReportOwnerId == userId)
            .Select(o => new
            {
                o.StatusId,
                o.Investigation,
            })
            .FirstOrDefaultAsync(ct);

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
            ReportOwnerAccessUntil = now.AddMonths(_gisSettings.AccessTokenIssueDurationMonths),
        };

        context.FloodReports.Add(floodReport);

        // Publish a created message to the message system?
        var message = floodReport.ToMessageCreated();
        await publishEndpoint.Publish(message, ct);

        // Add both the flood report and the message to the database
        await context.SaveChangesAsync(ct);

        return floodReport;
    }

    public async Task<FloodReport> CreateWithEligiblityCheck(EligibilityCheckDto dto, CancellationToken ct)
    {
        logger.LogInformation("Creating a new flood report with eligibility check.");

        var eligibilityCheckId = Guid.CreateVersion7();
        var now = DateTimeOffset.UtcNow;

        var impactDuration = await GetImpactDurationHours(dto.OnGoing, dto.DurationKnownId, dto.ImpactDuration, ct);

        var floodReport = new FloodReport
        {
            Reference = CreateReference(),
            CreatedUtc = now,
            StatusId = RecordStatusIds.New,
            ReportOwnerAccessUntil = now.AddMonths(_gisSettings.AccessTokenIssueDurationMonths),
            EligibilityCheck = new()
            {
                Id = eligibilityCheckId,
                CreatedUtc = now,
                TermsAgreed = now,

                IsAddress = dto.IsAddress,
                Uprn = dto.Uprn,
                Usrn = dto.Usrn,
                Easting = dto.Easting,
                Northing = dto.Northing,
                LocationDesc = dto.LocationDesc,
                TemporaryUprn = dto.TemporaryUprn,
                TemporaryLocationDesc = dto.TemporaryLocationDesc,
                ImpactStart = dto.ImpactStart,
                ImpactDuration = impactDuration,
                OnGoing = dto.OnGoing,
                Uninhabitable = dto.Uninhabitable == true,
                VulnerablePeopleId = dto.VulnerablePeopleId,
                VulnerableCount = dto.VulnerableCount,
                Residentials = [.. dto.Residentials.Select(floodImpactId => new EligibilityCheckResidential(eligibilityCheckId, floodImpactId))],
                Commercials = [.. dto.Commercials.Select(floodImpactId => new EligibilityCheckCommercial(eligibilityCheckId, floodImpactId))],
                Sources = [.. dto.Sources.Select(floodProblemId => new EligibilityCheckSource(eligibilityCheckId, floodProblemId))],
                SecondarySources = [.. dto.SecondarySources.Select(floodProblemId => new EligibilityCheckRunoffSource(eligibilityCheckId, floodProblemId))]
            },
        };

        // Add the flood report to the database
        context.FloodReports.Add(floodReport);

        // Publish mutiple messages to the message system
        var responsibleOrganisations = await commonRepository
            .GetResponsibleOrganisations(floodReport.EligibilityCheck.Easting, floodReport.EligibilityCheck.Northing, ct);
        var floodReportCreatedMessage = floodReport.ToMessageCreated();

        var fullFloodSource = await commonRepository
            .GetFullEligibilityFloodProblemSourceList(floodReport.EligibilityCheck, ct);
        var eligibilityCheckCreatedMessage = floodReport.EligibilityCheck.ToMessageCreated(floodReport.Reference, responsibleOrganisations, fullFloodSource);

        await publishEndpoint.Publish(floodReportCreatedMessage, ct);
        await publishEndpoint.Publish(eligibilityCheckCreatedMessage, ct);

        // Save the flood report, eligibility check, and messages to the database
        await context.SaveChangesAsync(ct);

        return floodReport;
    }

    public async Task<EligibilityResult> CalculateEligibilityWithReference(string reference, CancellationToken ct)
    {
        logger.LogInformation("Calculating eligibility for flood report reference {Reference}", reference);

        var floodReport = await context.FloodReports
            .AsNoTracking()
            .Include(o => o.ContactRecords)
            .Include(o => o.EligibilityCheck)
            .Where(o => o.Reference == reference)
            .FirstOrDefaultAsync(ct);

        if (floodReport is null)
        {
            logger.LogWarning("No flood report found for reference {Reference}", reference);
            throw new InvalidOperationException($"No flood report found for reference {reference}");
        }

        if (floodReport.EligibilityCheck is null)
        {
            logger.LogWarning("No eligibility check found for flood report reference {Reference}", reference);
            throw new InvalidOperationException($"No eligibility check found for flood report reference {reference}");
        }

        var responsibleOrganisations = await commonRepository
                .GetResponsibleOrganisations(floodReport.EligibilityCheck.Easting, floodReport.EligibilityCheck.Northing, ct);

        return new EligibilityResult
        {
            HasContactInformation = floodReport.ContactRecords.Any(),
            FloodInvestigation = floodReport.EligibilityCheck.IsInternal() ? EligibilityOptions.Conditional : EligibilityOptions.None,
            ResponsibleOrganisations = responsibleOrganisations,
            FloodReportId = floodReport.Id,

            // These don't have any logic yet
            IsEmergencyResponse = false,
            Section19Url = null,
            Section19 = EligibilityOptions.None,
            PropertyProtection = EligibilityOptions.None,
            GrantApplication = EligibilityOptions.None,
        };
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
        if (durationKnownId == FloodDurationIds.DurationKnown)
        {
            logger.LogInformation("Impact duration is known, using provided impact duration hours.");
            return impactDurationHours ?? 0;
        }

        // Get the duration from the flood problem
        var floodProblem = await context.FloodProblems.FindAsync([durationKnownId], ct);

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

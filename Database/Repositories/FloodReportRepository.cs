using FloodOnlineReportingTool.Database.DbContexts;
using FloodOnlineReportingTool.Database.Models;
using FloodOnlineReportingTool.Database.Settings;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FloodOnlineReportingTool.Database.Repositories;

public class FloodReportRepository(
    ILogger<FloodReportRepository> logger,
    FORTDbContext context,
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

                Uprn = dto.Uprn,
                Easting = dto.Easting,
                Northing = dto.Northing,
                LocationDesc = dto.LocationDesc,
                ImpactStart = dto.ImpactStart,
                ImpactDuration = dto.ImpactDuration ?? 0,
                OnGoing = dto.OnGoing,
                Uninhabitable = dto.Uninhabitable == true,
                VulnerablePeopleId = dto.VulnerablePeopleId,
                VulnerableCount = dto.VulnerableCount,

                // Add the related residential flood impacts
                Residentials = [.. dto.Residentials.Select(floodImpactId => new EligibilityCheckResidential(eligibilityCheckId, floodImpactId))],
                // Add the related commercial flood impacts
                Commercials = [.. dto.Commercials.Select(floodImpactId => new EligibilityCheckCommercial(eligibilityCheckId, floodImpactId))],
                // Add the related source flood problems
                Sources = [.. dto.Sources.Select(floodProblemId => new EligibilityCheckSource(eligibilityCheckId, floodProblemId))],
            },
        };

        // Add the flood report to the database
        context.FloodReports.Add(floodReport);

        // Publish mutiple messages to the message system
        var floodReportCreatedMessage = floodReport.ToMessageCreated();
        var eligibilityCheckCreatedMessage = floodReport.EligibilityCheck.ToMessageCreated(floodReport.Reference);
        await publishEndpoint
            .Publish(floodReportCreatedMessage, ct)
            .ConfigureAwait(false);
        await publishEndpoint
            .Publish(eligibilityCheckCreatedMessage, ct)
            .ConfigureAwait(false);

        // Save the flood report, eligibility check, and messages to the database
        await context
            .SaveChangesAsync(ct)
            .ConfigureAwait(false);

        return floodReport;
    }

    public bool HasInvestigationStarted(Guid status)
    {
        return status == RecordStatusIds.ActionNeeded;
    }
}

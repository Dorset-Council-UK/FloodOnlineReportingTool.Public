using FloodOnlineReportingTool.Contracts;
using FloodOnlineReportingTool.Contracts.Shared;
using FloodOnlineReportingTool.Database.DbContexts;
using FloodOnlineReportingTool.Database.Models.Eligibility;
using FloodOnlineReportingTool.Database.Models.Flood;
using FloodOnlineReportingTool.Database.Models.ResultModels;
using FloodOnlineReportingTool.Database.Options;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FloodOnlineReportingTool.Database.Repositories;

public class FloodReportRepository(
    ILogger<FloodReportRepository> logger,
    ICommonRepository commonRepository,
    IPublishEndpoint publishEndpoint,
    IOptions<GISOptions> options,
    PublicDbContext dbContext,
    IDbContextFactory<PublicDbContext> contextFactory
) : IFloodReportRepository
{
    private readonly GISOptions _gisOptions = options.Value;

    public async Task<FloodReport?> ReportedByUser(string userId, CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);

        return await context.ContactRecords
            .AsNoTracking()
            .AsSplitQuery()
            .Where(cr => cr.ContactUserId == userId)
            .SelectMany(cr => cr.FloodReports)
            .Include(o => o.EligibilityCheck)
                .ThenInclude(ec => ec.Residentials)
            .Include(o => o.EligibilityCheck)
                .ThenInclude(ec => ec.Commercials)
            .Include(o => o.EligibilityCheck)
                .ThenInclude(ec => ec.Sources)
            .Include(o => o.EligibilityCheck)
                .ThenInclude(ec => ec.SecondarySources)
            .Include(o => o.Investigation)
            .Include(o => o.ContactRecords)
            .Include(o => o.Status)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<FloodReport?> ReportedByContact(string contactUserId, Guid floodReportId, CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);

        return await context.ContactRecords
            .AsNoTracking()
            .AsSplitQuery()
            .Where(cr => cr.ContactUserId == contactUserId)
            .SelectMany(cr => cr.FloodReports)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyCollection<FloodReport>> AllReportedByContact(string contactUserId, CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);

        return await context.ContactRecords
            .AsNoTracking()
            .AsSplitQuery()
            .Where(cr => cr.ContactUserId == contactUserId)
            .SelectMany(cr => cr.FloodReports)
            .IgnoreAutoIncludes()     // Might need to remove this if we actually want sources and areas flooded. 
            .Include(o => o.EligibilityCheck)
            .Include(o => o.ContactRecords)
            .Include(o => o.Status)
            .OrderByDescending(cr => cr.CreatedUtc)
            .ToListAsync(ct);
    }

    public async Task<CreateOrUpdateResult<FloodReport>> EnableContactSubscriptionsForReport(Guid floodReportId, CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);

        var floodReport = await context.FloodReports
            .Include(fr => fr.ContactRecords)
                .ThenInclude(cr => cr.SubscribeRecords)
            .Include(fr => fr.EligibilityCheck)
            .FirstOrDefaultAsync(fr => fr.Id == floodReportId, ct);

        if (floodReport == null)
        {
            return CreateOrUpdateResult<FloodReport>.Failure(new List<string> { $"Flood report with id {floodReportId} not found." });
        }
        foreach(var contactRecord in floodReport.ContactRecords)
        {
            foreach (var subscriptionRecord in contactRecord.SubscribeRecords)
            {
                subscriptionRecord.IsSubscribed = true;
            }
        }

        await context.SaveChangesAsync(ct);
        return CreateOrUpdateResult<FloodReport>.Success(floodReport);
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

        await using var context = await contextFactory.CreateDbContextAsync(ct);

        // Include all related tables
        return await context.FloodReports
            .AsNoTracking()
            .AsSplitQuery()
            .Include(o => o.EligibilityCheck)
                .ThenInclude(ec => ec.Residentials)
            .Include(o => o.EligibilityCheck)
                .ThenInclude(ec => ec.Commercials)
            .Include(o => o.EligibilityCheck)
                .ThenInclude(ec => ec.Sources)
            .Include(o => o.EligibilityCheck)
                .ThenInclude(ec => ec.SecondarySources)
            .Include(o => o.Investigation)
            .Include(o => o.ContactRecords)
            .Include(o => o.Status)
            .FirstOrDefaultAsync(o => o.Id == reference, ct);
    }

    public async Task<FloodReport?> GetByReference(string reference, CancellationToken ct)
    {
        logger.LogInformation("Getting flood report by reference number {Reference}.", reference);

        await using var context = await contextFactory.CreateDbContextAsync(ct);

        // Include all related tables
        return await context.FloodReports
            .AsNoTracking()
            .AsSplitQuery()
            .Include(o => o.EligibilityCheck)
                .ThenInclude(ec => ec.Residentials)
            .Include(o => o.EligibilityCheck)
                .ThenInclude(ec => ec.Commercials)
            .Include(o => o.EligibilityCheck)
                .ThenInclude(ec => ec.Sources)
            .Include(o => o.EligibilityCheck)
                .ThenInclude(ec => ec.SecondarySources)
            .Include(o => o.Investigation)
            .Include(o => o.ContactRecords)
            .Include(o => o.Status)
            .FirstOrDefaultAsync(o => o.Reference == reference, ct);
    }

    public async Task<(bool hasFloodReport, bool hasInvestigation, bool hasInvestigationStarted, DateTimeOffset? investigationCreatedUtc)> InvestigationBasicInformation(Guid FloodReportId, CancellationToken ct)
    {
        logger.LogInformation("Getting flood report details by id.");

        // In simple terms only 2 fields are needed, StatusId and Investigation.CreatedUtc
        // Calling the standard ReportedByUser method is not efficient as it loads all related tables

        await using var context = await contextFactory.CreateDbContextAsync(ct);

        var result = await context.FloodReports
            .AsNoTracking()
            .Where(cr => cr.Id == FloodReportId)
            .Include(cr => cr.Investigation)
            .Select(o => new
            {
                o.StatusId,
                o.Investigation,
            })
            .FirstOrDefaultAsync(ct);

        if (result == null)
        {
            logger.LogWarning("No flood report found for user.");
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

    public async Task<FloodReport> CreateWithEligiblityCheck(EligibilityCheckDto dto, Uri viewUriBase, CancellationToken ct)
    {
        logger.LogInformation("Creating a new flood report with eligibility check.");

        await using var context = await contextFactory.CreateDbContextAsync(ct);

        var eligibilityCheckId = Guid.CreateVersion7();
        var now = DateTimeOffset.UtcNow;
        var impactDuration = await dto.CalculateImpactDurationHours(context, ct);

        var floodReport = new FloodReport
        {
            Reference = CreateReference(),
            CreatedUtc = now,
            StatusId = RecordStatusIds.New,
            ReportOwnerAccessUntil = now.AddMonths(_gisOptions.AccessTokenIssueDurationMonths),
            EligibilityCheck = dto.ToCreatedEntity(eligibilityCheckId, createdUtc: now, termsAgreed: now, impactDuration),
        };

        // Add the flood report to the database
        context.FloodReports.Add(floodReport);

        // Publish multiple messages to the message system
        var responsibleOrganisations = await commonRepository
            .GetResponsibleOrganisations(floodReport.EligibilityCheck.Easting, floodReport.EligibilityCheck.Northing, ct);
        var fullFloodSource = await commonRepository
           .GetFullEligibilityFloodProblemSourceList(floodReport.EligibilityCheck, ct);
        var eligibilityCheckRecord = floodReport.EligibilityCheck.ToMessageCreated(responsibleOrganisations, fullFloodSource);

        // Save the flood report, eligibility check, and messages to the database
        await context.SaveChangesAsync(ct);

        Uri viewUri = new Uri($"{viewUriBase}/{floodReport.Reference}");
        var floodReportCreatedMessage = floodReport.ToMessageCreated(viewUri, eligibilityCheckRecord);
        await SendBusMessages(floodReportCreatedMessage, ct);

        return floodReport;
    }

    private async Task SendBusMessages(FloodReportSourceCreated floodReportCreatedMessage, CancellationToken ct)
    {
        // Separate method as we need to use the generic context for MassTransit to pickup the request
        // and write to the outbox table.
        await publishEndpoint.Publish(floodReportCreatedMessage, ct);
        await dbContext.SaveChangesAsync(ct);
    }

    public async Task<EligibilityResult> CalculateEligibilityWithReference(string reference, CancellationToken ct)
    {
        logger.LogInformation("Calculating eligibility for flood report reference {Reference}", reference);

        await using var context = await contextFactory.CreateDbContextAsync(ct);

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
            HasContactInformation = floodReport.ContactRecords.Count > 0,
            FloodInvestigation = floodReport.EligibilityCheck.IsInternal ? EligibilityOptions.Conditional : EligibilityOptions.None,
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
}

using FloodOnlineReportingTool.Contracts;
using FloodOnlineReportingTool.Contracts.Shared;
using FloodOnlineReportingTool.Database.DbContexts;
using FloodOnlineReportingTool.Database.Models.Eligibility;
using FloodOnlineReportingTool.Database.Models.Flood;
using FloodOnlineReportingTool.Database.Models.Messaging;
using FloodOnlineReportingTool.Database.Models.ResultModels;
using FloodOnlineReportingTool.Database.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text.Json;

namespace FloodOnlineReportingTool.Database.Repositories;

public class FloodReportRepository(
    ILogger<FloodReportRepository> logger,
    ICommonRepository commonRepository,
    IOptions<GISOptions> options,
    IDbContextFactory<PublicDbContext> contextFactory
) : IFloodReportRepository
{
    private readonly GISOptions _gisOptions = options.Value;
    private readonly JsonSerializerOptions _jsonOptions = JsonSerializerOptions.Web;

    public async Task<IReadOnlyCollection<FloodReport>> ReportedByUser(string userId, CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);

        // EligibilityCheck and Investigation auto include
        return await context.ContactRecords
            .AsNoTracking()
            .AsSplitQuery()
            .Where(cr => cr.ContactUserId == userId)
            .SelectMany(cr => cr.FloodReports)
            .Include(fr => fr.ContactRecords)
            .Include(fr => fr.EligibilityCheck)
            .Include(fr => fr.Investigation)
            .Include(fr => fr.Status)
            .OrderByDescending(fr => fr.CreatedUtc)
            .ToListAsync(ct);
    }

    public async Task<FloodReport?> ReportedByUser(string userId, Guid floodReportId, CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);

        // EligibilityCheck and Investigation auto include
        return await context.ContactRecords
            .AsNoTracking()
            .AsSplitQuery()
            .Where(cr => cr.ContactUserId == userId)
            .SelectMany(cr => cr.FloodReports)
            .Where(fr => fr.Id == floodReportId)
            .Include(fr => fr.ContactRecords)
            .Include(fr => fr.EligibilityCheck)
            .Include(fr => fr.Investigation)
            .Include(fr => fr.Status)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<FloodReport?> ReportedByUser(string userId, string reference, CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);

        // EligibilityCheck and Investigation auto include
        return await context.ContactRecords
            .AsNoTracking()
            .AsSplitQuery()
            .Where(cr => cr.ContactUserId == userId)
            .SelectMany(cr => cr.FloodReports)
            .Where(fr => fr.Reference == reference)
            .Include(fr => fr.ContactRecords)
            .Include(fr => fr.EligibilityCheck)
            .Include(fr => fr.Investigation)
            .Include(fr => fr.Status)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<Result<FloodReport>> EnableContactSubscriptionsForReport(Guid floodReportId, CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);

        var floodReport = await context.FloodReports
            .Include(fr => fr.ContactRecords)
                .ThenInclude(cr => cr.SubscribeRecords)
            .Include(fr => fr.EligibilityCheck)
            .FirstOrDefaultAsync(fr => fr.Id == floodReportId, ct);

        if (floodReport == null)
        {
            return Result<FloodReport>.Failure([$"Flood report with id {floodReportId} not found."]);
        }
        foreach(var contactRecord in floodReport.ContactRecords)
        {
            foreach (var subscriptionRecord in contactRecord.SubscribeRecords)
            {
                subscriptionRecord.IsSubscribed = true;
            }
        }

        await context.SaveChangesAsync(ct);
        return Result<FloodReport>.Success(floodReport);
    }

    private async Task<string> CreateReference(CancellationToken ct)
    {
        var reference = GenerateReferenceId();
        //check existence of reference
        var reportWithReferenceExists = await ReportWithReferenceExists(reference, ct);
        var iteration = 0;
        var maxIterations = 100;
        while (reportWithReferenceExists && iteration < maxIterations)
        {
            reference = GenerateReferenceId();
            reportWithReferenceExists = await ReportWithReferenceExists(reference, ct);
            iteration++;
        }
        if(!reportWithReferenceExists)
        {
            logger.LogInformation("Creating a new flood report reference number {Reference}.", reference);
            return reference;
        }

        logger.LogError("Could not generate a unique reference after {maxIterations} tries", maxIterations);
        throw new Exception("Could not generate a unique reference");
    }

    public async Task<IReadOnlyCollection<FloodReport>> GetAllOverview(CancellationToken ct)
    {
        logger.LogInformation("Getting all flood reports, with simple overview information.");

        await using var context = await contextFactory.CreateDbContextAsync(ct);

        return await context.FloodReports
            .AsNoTracking()
            .IgnoreAutoIncludes()
            .Include(o => o.Status)
            .Include(o => o.EligibilityCheck)
            .OrderByDescending(o => o.CreatedUtc)
            .ToListAsync(ct);
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

    public async Task<bool> ReportWithReferenceExists(string reference, CancellationToken ct)
    {
        logger.LogInformation("Checking existence of flood report with reference number {Reference}.", reference);
        await using var context = await contextFactory.CreateDbContextAsync(ct);
        return await context.FloodReports
            .AsNoTracking()
            .AnyAsync(o => o.Reference == reference, ct);
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

    public async Task<Result<FloodReport>> Create(EligibilityCheckDto dto, Uri viewUriBase, CancellationToken ct)
    {
        logger.LogInformation("Creating a new flood report with eligibility check.");

        await using var context = await contextFactory.CreateDbContextAsync(ct);

        // Create eligibility check
        var now = DateTimeOffset.UtcNow;
        var impactDuration = await dto.CalculateImpactDurationHours(context, ct);
        EligibilityCheck eligibilityCheck = dto.ToCreatedEntity(Guid.CreateVersion7(), createdUtc: now, termsAgreed: now, impactDuration);

        // Create flood report
        var floodReport = new FloodReport
        {
            Reference = await CreateReference(ct),
            CreatedUtc = now,
            StatusId = RecordStatusIds.New,
            ReportOwnerAccessUntil = now.AddMonths(_gisOptions.AccessTokenIssueDurationMonths),
            EligibilityCheck = eligibilityCheck,
        };

        // Create a message
        FloodReportSourceCreated message = new(
            floodReport.Id,
            Buffer: 25,
            floodReport.Reference,
            ViewUri: new Uri(viewUriBase, $"details/{Uri.EscapeDataString(floodReport.Reference)}"),
            floodReport.CreatedUtc,
            eligibilityCheck.ToEligibilityCheckRecord(
                await commonRepository.GetResponsibleOrganisations(eligibilityCheck.Easting, eligibilityCheck.Northing, ct),
                await commonRepository.GetFullEligibilityFloodProblemSourceList(eligibilityCheck, ct)
            ),
            floodReport.Investigation is not null,
            floodReport.ContactRecords.Count > 0,
            [.. floodReport.ContactRecords
                .SelectMany(c => c.SubscribeRecords)
                .Select(s => s.ContactType)
                .Distinct(),
            ]
        );
        OutboxMessage outboxMessage = new()
        {
            MessageType = nameof(FloodReportSourceCreated),
            Message = JsonSerializer.Serialize(message, _jsonOptions),
            Status = MessageStatus.Pending,
        };

        try
        {
            // Save all changes
            context.FloodReports.Add(floodReport);
            context.OutboxMessages.Add(outboxMessage);
            await context.SaveChangesAsync(ct);

            return Result<FloodReport>.Success(floodReport);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating flood report with eligibility check.");
            return Result<FloodReport>.Failure(["Sorry there was a problem saving your flood report. Please try again but if this issue happens again then please report a bug."]);
        }
    }

    public Task<Result<FloodReport?>> Update(Guid id, EligibilityCheckDto dto, CancellationToken ct)
    {
        throw new NotImplementedException("Updating flood reports is not implemented yet.");

        //logger.LogInformation("Update eligibility check {Id}", id);

        //var eligibilityCheck = await context.EligibilityChecks
        //    .AsNoTracking()
        //    .FirstOrDefaultAsync(o => o.Id == id, ct);

        //if (eligibilityCheck == null)
        //{
        //    return null;
        //}

        //// Update eligiblity check
        //var impactDuration = await dto.CalculateImpactDurationHours(context, ct);
        //var updatedCheck = dto.ToUpdatedEntity(eligibilityCheck, updatedUtc: DateTimeOffset.UtcNow, impactDuration);
        //context.EligibilityChecks.Update(updatedCheck);

        //// Create a message

        //OutboxMessage outboxMessage = new()
        //{
        //    MessageType = nameof(FloodReportSourceCreated),
        //    Message = JsonSerializer.Serialize(message, _jsonOptions),
        //    Status = MessageStatus.Pending,
        //};


        //// Publish a updated message to the message system
        //var responsibleOrganisations = await commonRepository.GetResponsibleOrganisations(updatedCheck.Easting, updatedCheck.Northing, ct);
        //var fullFloodSource = await commonRepository.GetFullEligibilityFloodProblemSourceList(updatedCheck, ct);
        //var updatedMessage = updatedCheck.ToMessageUpdated(responsibleOrganisations, fullFloodSource);

        //// TODO: add save message to outbox pattern back in

        //// Update the database with the eligibility check, message, flood impacts, and flood problems
        //await context.SaveChangesAsync(ct);

        //return updatedCheck;
    }

    public Task<Result<FloodReport?>> Update(string userId, Guid id, EligibilityCheckDto dto, CancellationToken ct)
    {
        throw new NotImplementedException("Updating flood reports is not implemented yet.");

        //var eligibilityCheck = await context.ContactRecords
        //    .AsNoTracking()
        //    .Where(cr => cr.ContactUserId == userId)
        //    .SelectMany(cr => cr.FloodReports)
        //    .Select(fr => fr.EligibilityCheck)
        //    .FirstOrDefaultAsync(o => o != null && o.Id == id, ct)
        //    ?? throw new InvalidOperationException("No eligiblity check found");

        //// Update the fields we choose
        //var impactDuration = await dto.CalculateImpactDurationHours(context, ct);
        //var updatedCheck = dto.ToUpdatedEntity(eligibilityCheck, updatedUtc: DateTimeOffset.UtcNow, impactDuration);
        //context.EligibilityChecks.Update(updatedCheck);

        //// Publish a updated message to the message system
        //var responsibleOrganisations = await commonRepository.GetResponsibleOrganisations(updatedCheck.Easting, updatedCheck.Northing, ct);
        //var fullFloodSource = await commonRepository.GetFullEligibilityFloodProblemSourceList(updatedCheck, ct);
        //var updatedMessage = updatedCheck.ToMessageUpdated(responsibleOrganisations, fullFloodSource);

        //// TODO: add save message to outbox pattern back in

        //// Update the database with the eligibility check, message, flood impacts, and flood problems
        //await context.SaveChangesAsync(ct);

        //return updatedCheck;
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

    private const string CrockfordAlphabet = "0123456789ABCDEFGHJKMNPQRSTVWXYZ";

    private static string GenerateReferenceId()
    {
        var bytes = RandomNumberGenerator.GetBytes(5); // 40 bits
        ulong value = ((ulong)bytes[0] << 32) | ((ulong)bytes[1] << 24) |
                      ((ulong)bytes[2] << 16) | ((ulong)bytes[3] << 8) | bytes[4];

        Span<char> chars = stackalloc char[8];
        for (int i = 7; i >= 0; i--)
        {
            chars[i] = CrockfordAlphabet[(int)(value & 0x1F)];
            value >>= 5;
        }
        return new string(chars);
    }
}

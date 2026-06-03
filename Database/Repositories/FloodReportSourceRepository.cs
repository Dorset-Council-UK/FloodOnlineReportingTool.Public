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

public class FloodReportSourceRepository(
    ILogger<FloodReportSourceRepository> logger,
    ICommonRepository commonRepository,
    IOptions<GISOptions> options,
    IDbContextFactory<PublicDbContext> contextFactory
) : IFloodReportSourceRepository
{
    private readonly GISOptions _gisOptions = options.Value;
    private readonly JsonSerializerOptions _jsonOptions = JsonSerializerOptions.Web;

    public async Task<int> Count(CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.FloodReportSources.CountAsync(cancellationToken);
    }

    public async Task<int> Count(string userId, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.FloodReportSources
            .Where(frs => frs.ContactRecords.Any(cr => cr.ContactUserId == userId))
            .CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<FloodReportSource>> ReportedByUser(string userId, CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);

        // EligibilityCheck and Investigation auto include
        return await context.ContactRecords
            .AsNoTracking()
            .AsSplitQuery()
            .Where(cr => cr.ContactUserId == userId)
            .SelectMany(cr => cr.FloodReportSources)
            .Include(frs => frs.ContactRecords)
            .Include(frs => frs.EligibilityCheck)
            .Include(frs => frs.Investigation)
            .Include(frs => frs.Status)
            .OrderByDescending(frs => frs.CreatedUtc)
            .ToListAsync(ct);
    }

    public async Task<FloodReportSource?> ReportedByUser(string userId, Guid floodReportSourceId, CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);

        // EligibilityCheck and Investigation auto include
        return await context.ContactRecords
            .AsNoTracking()
            .AsSplitQuery()
            .Where(cr => cr.ContactUserId == userId)
            .SelectMany(cr => cr.FloodReportSources)
            .Where(frs => frs.Id == floodReportSourceId)
            .Include(frs => frs.ContactRecords)
            .Include(frs => frs.EligibilityCheck)
            .Include(frs => frs.Investigation)
            .Include(frs => frs.Status)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<FloodReportSource?> ReportedByUser(string userId, string reference, CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);

        // EligibilityCheck and Investigation auto include
        return await context.ContactRecords
            .AsNoTracking()
            .AsSplitQuery()
            .Where(cr => cr.ContactUserId == userId)
            .SelectMany(cr => cr.FloodReportSources)
            .Where(frs => frs.Reference == reference)
            .Include(frs => frs.ContactRecords)
            .Include(frs => frs.EligibilityCheck)
            .Include(frs => frs.Investigation)
            .Include(frs => frs.Status)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<Result<FloodReportSource>> EnableContactSubscriptionsForReport(Guid floodReportSourceId, CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);

        var floodReportSource = await context.FloodReportSources
            .Include(frs => frs.ContactRecords)
                .ThenInclude(cr => cr.SubscribeRecords)
            .Include(frs => frs.EligibilityCheck)
            .FirstOrDefaultAsync(frs => frs.Id == floodReportSourceId, ct);

        if (floodReportSource == null)
        {
            logger.LogInformation("Flood report source with ID {FloodReportSourceId} not found", floodReportSourceId);
            return Result<FloodReportSource>.Failure(["Flood report not found."]);
        }
        foreach(var contactRecord in floodReportSource.ContactRecords)
        {
            foreach (var subscriptionRecord in contactRecord.SubscribeRecords)
            {
                subscriptionRecord.IsSubscribed = true;
            }
        }

        await context.SaveChangesAsync(ct);
        return Result<FloodReportSource>.Success(floodReportSource);
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
            logger.LogInformation("Creating a new flood report source reference number {Reference}.", reference);
            return reference;
        }

        logger.LogError("Could not generate a unique reference after {maxIterations} tries", maxIterations);
        throw new Exception("Could not generate a unique reference");
    }

    public async Task<IReadOnlyCollection<FloodReportSource>> GetAllOverview(CancellationToken ct)
    {
        logger.LogInformation("Getting all flood report sources, with simple overview information.");

        await using var context = await contextFactory.CreateDbContextAsync(ct);

        return await context.FloodReportSources
            .AsNoTracking()
            .IgnoreAutoIncludes()
            .Include(frs => frs.Status)
            .Include(frs => frs.EligibilityCheck)
            .OrderByDescending(frs => frs.CreatedUtc)
            .ToListAsync(ct);
    }

    public async Task<FloodReportSource?> GetById(Guid floodReportSourceId, CancellationToken ct)
    {
        logger.LogInformation("Getting flood report source by ID: {FloodReportSourceId}.", floodReportSourceId);

        await using var context = await contextFactory.CreateDbContextAsync(ct);

        // Include all related tables
        return await context.FloodReportSources
            .AsNoTracking()
            .AsSplitQuery()
            .Include(frs => frs.EligibilityCheck)
                .ThenInclude(ec => ec.Residentials)
            .Include(frs => frs.EligibilityCheck)
                .ThenInclude(ec => ec.Commercials)
            .Include(frs => frs.EligibilityCheck)
                .ThenInclude(ec => ec.Sources)
            .Include(frs => frs.EligibilityCheck)
                .ThenInclude(ec => ec.SecondarySources)
            .Include(frs => frs.Investigation)
            .Include(frs => frs.ContactRecords)
            .Include(frs => frs.Status)
            .FirstOrDefaultAsync(frs => frs.Id == floodReportSourceId, ct);
    }

    public async Task<FloodReportSource?> GetByReference(string reference, CancellationToken ct)
    {
        logger.LogInformation("Getting flood report source by reference number {Reference}.", reference);

        await using var context = await contextFactory.CreateDbContextAsync(ct);

        // Include all related tables
        return await context.FloodReportSources
            .AsNoTracking()
            .AsSplitQuery()
            .Include(frs => frs.EligibilityCheck)
                .ThenInclude(ec => ec.Residentials)
            .Include(frs => frs.EligibilityCheck)
                .ThenInclude(ec => ec.Commercials)
            .Include(frs => frs.EligibilityCheck)
                .ThenInclude(ec => ec.Sources)
            .Include(frs => frs.EligibilityCheck)
                .ThenInclude(ec => ec.SecondarySources)
            .Include(frs => frs.Investigation)
            .Include(frs => frs.ContactRecords)
            .Include(frs => frs.Status)
            .FirstOrDefaultAsync(frs => frs.Reference == reference, ct);
    }

    public async Task<bool> ReportWithReferenceExists(string reference, CancellationToken ct)
    {
        logger.LogInformation("Checking existence of flood report source with reference: {Reference}.", reference);
        await using var context = await contextFactory.CreateDbContextAsync(ct);
        return await context.FloodReportSources.AnyAsync(frs => frs.Reference == reference, ct);
    }

    public async Task<(bool hasFloodReportSource, bool hasInvestigation, bool hasInvestigationStarted, DateTimeOffset? investigationCreatedUtc)> InvestigationBasicInformation(Guid floodReportSourceId, CancellationToken ct)
    {
        logger.LogInformation("Getting flood report source details by ID: {FloodReportSourceId}", floodReportSourceId);

        // In simple terms only 2 fields are needed, StatusId and Investigation.CreatedUtc
        // Calling the standard ReportedByUser method is not efficient as it loads all related tables

        await using var context = await contextFactory.CreateDbContextAsync(ct);

        var result = await context.FloodReportSources
            .AsNoTracking()
            .Where(frs => frs.Id == floodReportSourceId)
            .Include(frs => frs.Investigation)
            .Select(frs => new
            {
                frs.StatusId,
                frs.Investigation,
            })
            .FirstOrDefaultAsync(ct);

        if (result == null)
        {
            logger.LogWarning("No flood report source found for user.");
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

    public async Task<Result<FloodReportSource>> Create(EligibilityCheckDto dto, Uri viewUriBase, CancellationToken ct)
    {
        logger.LogInformation("Creating a new flood report source with eligibility check.");

        await using var context = await contextFactory.CreateDbContextAsync(ct);

        // Create eligibility check
        var now = DateTimeOffset.UtcNow;
        var impactDuration = await dto.CalculateImpactDurationHours(context, ct);
        EligibilityCheck eligibilityCheck = dto.ToCreatedEntity(Guid.CreateVersion7(), createdUtc: now, termsAgreed: now, impactDuration);

        // Create flood report source
        var floodReportSource = new FloodReportSource
        {
            Reference = await CreateReference(ct),
            CreatedUtc = now,
            StatusId = RecordStatusIds.New,
            ReportOwnerAccessUntil = now.AddMonths(_gisOptions.AccessTokenIssueDurationMonths),
            EligibilityCheck = eligibilityCheck,
        };

        // Create a message
        FloodReportSourceCreated message = new(
            floodReportSource.Id,
            Buffer: 25,
            floodReportSource.Reference,
            ViewUri: new Uri(viewUriBase, $"details/{Uri.EscapeDataString(floodReportSource.Reference)}"),
            floodReportSource.CreatedUtc,
            eligibilityCheck.ToEligibilityCheckRecord(
                await commonRepository.GetResponsibleOrganisations(eligibilityCheck.Easting, eligibilityCheck.Northing, ct),
                await commonRepository.GetFullEligibilityFloodProblemSourceList(eligibilityCheck, ct)
            ),
            floodReportSource.Investigation is not null,
            floodReportSource.ContactRecords.Count > 0,
            [.. floodReportSource.ContactRecords
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
            context.FloodReportSources.Add(floodReportSource);
            context.OutboxMessages.Add(outboxMessage);
            await context.SaveChangesAsync(ct);

            return Result<FloodReportSource>.Success(floodReportSource);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating flood report source with eligibility check.");
            return Result<FloodReportSource>.Failure(["Sorry there was a problem saving your flood report. Please try again but if this issue happens again then please report a bug."]);
        }
    }

    public async Task<Result<FloodReportSource?>> Update(Guid eligibilityCheckId, EligibilityCheckDto dto, Guid status, Uri viewUriBase, CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);

        // Find the flood report source
        var floodReportSource = await context.FloodReportSources
            .AsNoTracking()
            .Include(frs => frs.EligibilityCheck)
            .FirstOrDefaultAsync(frs => frs.EligibilityCheck != null && frs.EligibilityCheck.Id == eligibilityCheckId, ct);

        if (floodReportSource is null)
        {
            logger.LogWarning("No flood report source found for eligibility check id {EligibilityCheckId}", eligibilityCheckId);
            return Result<FloodReportSource?>.Failure([$"No flood report found for eligibility check id {eligibilityCheckId}."]);
        }
        if (floodReportSource.EligibilityCheck is null)
        {
            logger.LogWarning("No eligibility check found for id {EligibilityCheckId}", eligibilityCheckId);
            return Result<FloodReportSource?>.Failure([$"Eligibility check with id {eligibilityCheckId} not found."]);
        }

        // Update the eligibility check
        var updatedUtc = DateTimeOffset.UtcNow;
        var impactDuration = await dto.CalculateImpactDurationHours(context, ct);
        var updatedEligibilityCheck = dto.ToUpdatedEntity(floodReportSource.EligibilityCheck, updatedUtc, impactDuration);

        // Create a message
        FloodReportSourceUpdated message = new(
            floodReportSource.Id,
            floodReportSource.Reference,
            ViewUri: new Uri(viewUriBase, $"details/{Uri.EscapeDataString(floodReportSource.Reference)}"),
            updatedUtc,
            status,
            EligibilityCheckRecord: null, // Temporary: this is going to be removed
            ActionStatusUpdates: [] // Temporary: this is going to be removed or changed
        );
        OutboxMessage outboxMessage = new()
        {
            MessageType = nameof(FloodReportSourceUpdated),
            Message = JsonSerializer.Serialize(message, _jsonOptions),
            Status = MessageStatus.Pending,
        };

        // Save all changes
        context.EligibilityChecks.Update(updatedEligibilityCheck);
        context.OutboxMessages.Add(outboxMessage);
        await context.SaveChangesAsync(ct);

        logger.LogInformation("Updated flood report source with eligibility check ID: {EligibilityCheckId}", eligibilityCheckId);
        return Result<FloodReportSource?>.Success(floodReportSource);
    }

    public async Task<Result<FloodReportSource?>> Update(string userId, Guid eligibilityCheckId, EligibilityCheckDto dto, Guid status, Uri viewUriBase, CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);

        // Find the users flood report source
        var floodReportSource = await context.ContactRecords
            .AsNoTracking()
            .Where(cr => cr.ContactUserId == userId)
            .SelectMany(cr => cr.FloodReportSources)
            .Include(frs => frs.EligibilityCheck)
            .FirstOrDefaultAsync(frs => frs.EligibilityCheck != null && frs.EligibilityCheck.Id == eligibilityCheckId, ct);

        if (floodReportSource is null)
        {
            logger.LogWarning("No flood report source found for user and eligibility check id {EligibilityCheckId}", eligibilityCheckId);
            return Result<FloodReportSource?>.Failure([$"No flood report found for eligibility check id {eligibilityCheckId}."]);
        }
        if (floodReportSource.EligibilityCheck is null)
        {
            logger.LogWarning("No eligibility check found for user and id {EligibilityCheckId}", eligibilityCheckId);
            return Result<FloodReportSource?>.Failure([$"Eligibility check with id {eligibilityCheckId} not found."]);
        }

        // Update the users eligibility check
        var updatedUtc = DateTimeOffset.UtcNow;
        var impactDuration = await dto.CalculateImpactDurationHours(context, ct);
        var updatedEligibilityCheck = dto.ToUpdatedEntity(floodReportSource.EligibilityCheck, updatedUtc, impactDuration);

        // Create a message
        FloodReportSourceUpdated message = new(
            floodReportSource.Id,
            floodReportSource.Reference,
            ViewUri: new Uri(viewUriBase, $"details/{Uri.EscapeDataString(floodReportSource.Reference)}"),
            updatedUtc,
            status,
            EligibilityCheckRecord: null, // Temporary: this is going to be removed
            ActionStatusUpdates: [] // Temporary: this is going to be removed or changed
        );
        OutboxMessage outboxMessage = new()
        {
            MessageType = nameof(FloodReportSourceUpdated),
            Message = JsonSerializer.Serialize(message, _jsonOptions),
            Status = MessageStatus.Pending,
        };

        // Save all changes
        context.EligibilityChecks.Update(updatedEligibilityCheck);
        context.OutboxMessages.Add(outboxMessage);
        await context.SaveChangesAsync(ct);

        logger.LogInformation("Updated users flood report source with eligibility check id {EligibilityCheckId}", eligibilityCheckId);
        return Result<FloodReportSource?>.Success(floodReportSource);
    }

    public async Task<EligibilityResult> CalculateEligibilityWithReference(string reference, CancellationToken ct)
    {
        logger.LogInformation("Calculating eligibility for flood report source reference {Reference}", reference);

        await using var context = await contextFactory.CreateDbContextAsync(ct);

        var floodReportSource = await context.FloodReportSources
            .AsNoTracking()
            .Include(frs => frs.ContactRecords)
            .Include(frs => frs.EligibilityCheck)
            .Where(frs => frs.Reference == reference)
            .FirstOrDefaultAsync(ct);

        if (floodReportSource is null)
        {
            logger.LogWarning("No flood report source found for reference {Reference}", reference);
            throw new InvalidOperationException($"No flood report found for reference {reference}");
        }

        if (floodReportSource.EligibilityCheck is null)
        {
            logger.LogWarning("No eligibility check found for flood report source reference {Reference}", reference);
            throw new InvalidOperationException($"No eligibility check found for flood report reference {reference}");
        }

        var responsibleOrganisations = await commonRepository
                .GetResponsibleOrganisations(floodReportSource.EligibilityCheck.Easting, floodReportSource.EligibilityCheck.Northing, ct);

        return new EligibilityResult
        {
            HasContactInformation = floodReportSource.ContactRecords.Count > 0,
            FloodInvestigation = floodReportSource.EligibilityCheck.IsInternal ? EligibilityOptions.Conditional : EligibilityOptions.None,
            ResponsibleOrganisations = responsibleOrganisations,
            FloodReportSourceId = floodReportSource.Id,

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

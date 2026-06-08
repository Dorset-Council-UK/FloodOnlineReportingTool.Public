using FloodOnlineReportingTool.Contracts.Shared;
using FloodOnlineReportingTool.Database.DbContexts;
using FloodOnlineReportingTool.Database.Models.Eligibility;
using FloodOnlineReportingTool.Database.Models.Investigate;
using FloodOnlineReportingTool.Database.Models.ResultModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FloodOnlineReportingTool.Database.Repositories;

public class InvestigationRepository(
    ILogger<InvestigationRepository> logger,
    IDbContextFactory<PublicDbContext> contextFactory
) : IInvestigationRepository
{
    public async Task<int> Count(CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Investigations.CountAsync(cancellationToken);
    }

    public async Task<Investigation?> ReportedByUser(string userId, Guid investigationId, CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);
        return await context.ContactRecords
            .AsNoTracking()
            .Where(cr => cr.ContactUserId == userId)
            .SelectMany(cr => cr.FloodReportSources)
            .Select(frs => frs.Investigation)
            .FirstOrDefaultAsync(i => i != null && i.Id == investigationId, ct);
    }

    public async Task<Result<Investigation>> CreateForFloodReportSource(string userId, InvestigationDto investigationDto, CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);
        var floodReportSource = await context.FloodReportSources
            .AsNoTracking()
            .Where(frs => frs.Id == investigationDto.FloodReportSourceId)
            .FirstOrDefaultAsync(ct);

        if (floodReportSource == null)
        {
            logger.LogInformation("No flood report source found for id {FloodReportSourceId}", investigationDto.FloodReportSourceId);
            return Result<Investigation>.Failure(["No flood report found"]);
        }
        if (floodReportSource.InvestigationId != null)
        {
            logger.LogInformation("An investigation already exists for flood report source ID {FloodReportSourceId}", investigationDto.FloodReportSourceId);
            return Result<Investigation>.Failure(["An investigation already exists for this flood report"]);
        }
        if (floodReportSource.StatusId != RecordStatusIds.ActionNeeded)
        {
            logger.LogInformation("Flood report source ID {FloodReportSourceId} is not in the correct status for an investigation to be created. Status ID: {StatusId}", investigationDto.FloodReportSourceId, floodReportSource.StatusId);
            return Result<Investigation>.Failure(["There is not currently an ongoing investigation for this flood report"]);
        }

        var investigation = CreateBaseInvestigation(investigationDto)
            .ApplyInternalFields(investigationDto, floodReportSource.EligibilityCheck?.IsInternal ?? false)
            .ApplyPeakDepth(investigationDto)
            .ApplyFloodlineWarnings(investigationDto);

        var updatedFloodReportSource = floodReportSource with
        {
            InvestigationId = investigation.Id,
            StatusId = RecordStatusIds.ActionCompleted,
        };

        context.Investigations.Add(investigation);
        context.FloodReportSources.Update(updatedFloodReportSource);

        // Publish a message to the message system
        // TODO: add save message to outbox pattern back in
        //var message = investigation.ToMessageCreated(floodReport.Reference);

        // Save the investigation in the database
        await context.SaveChangesAsync(ct);

        logger.LogInformation("Investigation with ID {InvestigationId} created for flood report source ID {FloodReportSourceId}", investigation.Id, investigationDto.FloodReportSourceId);
        return Result<Investigation>.Success(investigation);
    }

    public async Task<Investigation?> ReportedByUserBasicInformation(string userId, CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);
        return await context.ContactRecords
            .AsNoTracking()
            .Where(cr => cr.ContactUserId == userId)
            .SelectMany(cr => cr.FloodReportSources)
            .Select(frs => frs.Investigation)
            .FirstOrDefaultAsync(ct);
    }

    /// <summary>
    /// Creates the base investigation entity from the DTO.
    /// </summary>
    private static Investigation CreateBaseInvestigation(InvestigationDto dto)
    {
        var investigationId = Guid.CreateVersion7();
        return new Investigation
        {
            Id = investigationId,
            CreatedUtc = DateTimeOffset.UtcNow,

            // Water speed
            BeginId = dto.BeginId!.Value,
            WaterSpeedId = dto.WaterSpeedId!.Value,
            AppearanceId = dto.AppearanceId!.Value,
            MoreAppearanceDetails = dto.MoreAppearanceDetails,

            // Water destination
            Destinations = [.. dto.Destinations.Select(floodProblemId => new InvestigationDestination(investigationId, floodProblemId))],

            // Damaged vehicles
            WereVehiclesDamagedId = dto.WereVehiclesDamagedId!.Value,
            NumberOfVehiclesDamaged = dto.NumberOfVehiclesDamaged,

            // Internal (handled within ApplyInternalFields)

            // Peak depth (handled with ApplyPeakDepth)
            IsPeakDepthKnownId = Guid.Empty,

            // Service impacts
            ServiceImpacts = [.. dto.ServiceImpacts.Select(floodImpactId => new InvestigationServiceImpact(investigationId, floodImpactId))],

            // Community impact
            CommunityImpacts = [.. dto.CommunityImpacts.Select(floodImpactId => new InvestigationCommunityImpact(investigationId, floodImpactId))],

            // Blockages
            HasKnownProblems = dto.HasKnownProblems == true,
            KnownProblemDetails = dto.KnownProblemDetails,

            // Actions taken
            ActionsTaken = [.. dto.ActionsTaken.Select(floodMitigationId => new InvestigationActionsTaken(investigationId, floodMitigationId))],
            OtherAction = dto.OtherAction,

            // Help received
            HelpReceived = [.. dto.HelpReceived.Select(floodMitigationId => new InvestigationHelpReceived(investigationId, floodMitigationId))],

            // Before the flooding - Warnings
            FloodlineId = dto.FloodlineId!.Value,
            WarningReceivedId = dto.WarningReceivedId!.Value,

            // Warning sources
            WarningSources = [.. dto.WarningSources.Select(floodMitigationId => new InvestigationWarningSource(investigationId, floodMitigationId))],
            WarningSourceOther = dto.WarningSourceOther,

            // History
            HistoryOfFloodingId = dto.HistoryOfFloodingId!.Value,
            HistoryOfFloodingDetails = dto.HistoryOfFloodingDetails,
            PropertyInsuredId = dto.PropertyInsuredId!.Value,
        };
    }
}

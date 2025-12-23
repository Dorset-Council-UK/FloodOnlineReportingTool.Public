using FloodOnlineReportingTool.Contracts.Shared;
using FloodOnlineReportingTool.Database.DbContexts;
using FloodOnlineReportingTool.Database.Models.Eligibility;
using FloodOnlineReportingTool.Database.Models.Investigate;

using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace FloodOnlineReportingTool.Database.Repositories;

public class InvestigationRepository(PublicDbContext context, IPublishEndpoint publishEndpoint) : IInvestigationRepository
{
    public async Task<Investigation?> ReportedByUser(Guid userId, Guid id, CancellationToken ct)
    {
        return await context.ContactRecords
            .AsNoTracking()
            .Where(cr => cr.ContactUserId == userId)
            .SelectMany(cr => cr.FloodReports)
            .Select(o => o.Investigation)
            .FirstOrDefaultAsync(o => o != null && o.Id == id, ct);
    }

    public async Task<Investigation> CreateForUser(Guid userId, InvestigationDto investigationDto, CancellationToken ct)
    {
        var floodReport = await context.ContactRecords
            .AsNoTracking()
            .Where(cr => cr.ContactUserId == userId)
            .SelectMany(cr => cr.FloodReports)
            .FirstOrDefaultAsync(ct);

        if (floodReport == null)
        {
            throw new InvalidOperationException("No flood report found");
        }
        if (floodReport.InvestigationId != null)
        {
            throw new InvalidOperationException("An investigation already exists for this flood report");
        }
        if (floodReport.StatusId != RecordStatusIds.ActionNeeded)
        {
            throw new InvalidOperationException("There is not currently an ongoing investigation for this flood report");
        }

        var investigation = CreateBaseInvestigation(investigationDto)
            .ApplyInternalFields(investigationDto, floodReport.EligibilityCheck?.IsInternal() ?? false)
            .ApplyPeakDepth(investigationDto)
            .ApplyFloodlineWarnings(investigationDto);

        var updatedFloodReport = floodReport with
        {
            InvestigationId = investigation.Id,
            StatusId = RecordStatusIds.ActionCompleted,
        };

        context.Investigations.Add(investigation);
        context.FloodReports.Update(updatedFloodReport);

        // Publish a message to the message system
        var message = investigation.ToMessageCreated(floodReport.Reference);
        await publishEndpoint.Publish(message, ct);

        // Save the investigation in the database
        await context.SaveChangesAsync(ct);

        return investigation;
    }

    public async Task<Investigation?> ReportedByUserBasicInformation(Guid userId, CancellationToken ct)
    {
        return await context.ContactRecords
            .AsNoTracking()
            .Where(cr => cr.ContactUserId == userId)
            .SelectMany(cr => cr.FloodReports)
            .Select(o => o.Investigation)
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
        };
    }
}

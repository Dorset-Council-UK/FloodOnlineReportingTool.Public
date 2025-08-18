using FloodOnlineReportingTool.Database.DbContexts;
using FloodOnlineReportingTool.Database.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace FloodOnlineReportingTool.Database.Repositories;

public class InvestigationRepository(PublicDbContext context, IPublishEndpoint publishEndpoint) : IInvestigationRepository
{
    public async Task<Investigation?> ReportedByUser(Guid userId, Guid id, CancellationToken ct)
    {
        return await context.FloodReports
            .AsNoTracking()
            .Include(o => o.Investigation)
            .Where(o => o.ReportedByUserId == userId)
            .Select(o => o.Investigation)
            .FirstOrDefaultAsync(o => o != null && o.Id == id, ct)
            .ConfigureAwait(false);
    }

    public async Task<Investigation> CreateForUser(Guid userId, InvestigationDto investigationDto, CancellationToken ct)
    {
        var floodReport = await context.FloodReports
            .AsNoTracking()
            .Include(o => o.EligibilityCheck)
            .FirstOrDefaultAsync(o => o.ReportedByUserId == userId, ct)
            .ConfigureAwait(false);

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

        var isInternal = floodReport.EligibilityCheck?.IsInternal() ?? false;
        var investigationId = Guid.CreateVersion7();
        var investigation = investigationDto.ToInvestigation(investigationId, isInternal) with
        {
            CreatedUtc = DateTimeOffset.UtcNow,
        };

        var updatedFloodReport = floodReport with
        {
            InvestigationId = investigation.Id,
            StatusId = RecordStatusIds.ActionCompleted,
        };

        context.Investigations.Add(investigation);
        context.FloodReports.Update(updatedFloodReport);

        // Publish a message to the message system
        var message = investigation.ToMessageCreated(floodReport.Reference);
        await publishEndpoint
            .Publish(message, ct)
            .ConfigureAwait(false);

        // Save the investigation in the database
        await context
            .SaveChangesAsync(ct)
            .ConfigureAwait(false);

        return investigation;
    }

    public async Task<Investigation?> ReportedByUserBasicInformation(Guid userId, CancellationToken ct)
    {
        return await context.FloodReports
            .AsNoTracking()
            .IgnoreAutoIncludes()
            .Include(o => o.Investigation)
            .Where(o => o.ReportedByUserId == userId)
            .Select(o => o.Investigation)
            .FirstOrDefaultAsync(ct)
            .ConfigureAwait(false);
    }
}

using FloodOnlineReportingTool.Database.DbContexts;
using FloodOnlineReportingTool.Database.Models.Eligibility;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FloodOnlineReportingTool.Database.Repositories;

public class EligibilityCheckRepository(ILogger<EligibilityCheckRepository> logger, PublicDbContext context, IPublishEndpoint publishEndpoint, ICommonRepository commonRepository) : IEligibilityCheckRepository
{
    public async Task<EligibilityCheck?> ReportedByUser(string userId, CancellationToken ct)
    {
        // TODO: Might need to check a status on the flood report
        return await context.ContactRecords
            .AsNoTracking()
            .Where(cr => cr.ContactUserId == userId)
            .SelectMany(cr => cr.FloodReports)
            .Select(fr => fr.EligibilityCheck)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<EligibilityCheck?> ReportedByUser(string userId, Guid id, CancellationToken ct)
    {
        // TODO: Might need to check a status on the flood report
        return await context.ContactRecords
            .AsNoTracking()
            .Where(cr => cr.ContactUserId == userId)
            .SelectMany(cr => cr.FloodReports)
            .Select(fr => fr.EligibilityCheck)
            .FirstOrDefaultAsync(o => o != null && o.Id == id, ct);
    }

    public async Task<EligibilityCheck?> GetById(Guid id, CancellationToken ct)
    {
        logger.LogInformation("Getting eligibility check by id {Id}", id);

        // FloodProblems, and FloodImpacts will be auto included
        return await context.EligibilityChecks.FindAsync([id], ct);
    }

    public async Task<EligibilityCheck?> GetByReference(string reference, CancellationToken ct)
    {
        logger.LogInformation("Getting eligibility check by flood report reference {Reference}", reference.Replace(Environment.NewLine, "", StringComparison.OrdinalIgnoreCase));

        return await context.EligibilityChecks
            .AsNoTracking()
            .Include(o => o.FloodReport)
            .Where(o => o.FloodReport.Reference == reference)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<EligibilityCheck?> Update(Guid id, EligibilityCheckDto dto, CancellationToken ct)
    {
        logger.LogInformation("Update eligibility check {Id}", id);

        var eligibilityCheck = await context.EligibilityChecks
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id, ct);

        if (eligibilityCheck == null)
        {
            return null;
        }

        var impactDuration = await dto.CalculateImpactDurationHours(context, ct);
        var updatedCheck = dto.ToUpdatedEntity(eligibilityCheck, updatedUtc: DateTimeOffset.UtcNow, impactDuration);
        context.EligibilityChecks.Update(updatedCheck);

        // Publish a updated message to the message system
        var responsibleOrganisations = await commonRepository.GetResponsibleOrganisations(updatedCheck.Easting, updatedCheck.Northing, ct);
        var fullFloodSource = await commonRepository.GetFullEligibilityFloodProblemSourceList(updatedCheck, ct);
        var updatedMessage = updatedCheck.ToMessageUpdated(responsibleOrganisations, fullFloodSource);

        await publishEndpoint.Publish(updatedMessage, ct);

        // Update the database with the eligibility check, message, flood impacts, and flood problems
        await context.SaveChangesAsync(ct);

        return updatedCheck;
    }

    public async Task<EligibilityCheck> UpdateForUser(string userId, Guid id, EligibilityCheckDto dto, CancellationToken ct)
    {
        var eligibilityCheck = await context.ContactRecords
            .AsNoTracking()
            .Where(cr => cr.ContactUserId == userId)
            .SelectMany(cr => cr.FloodReports)
            .Select(fr => fr.EligibilityCheck)
            .FirstOrDefaultAsync(o => o != null && o.Id == id, ct)
            ?? throw new InvalidOperationException("No eligiblity check found");

        // Update the fields we choose
        var impactDuration = await dto.CalculateImpactDurationHours(context, ct);
        var updatedCheck = dto.ToUpdatedEntity(eligibilityCheck, updatedUtc: DateTimeOffset.UtcNow, impactDuration);
        context.EligibilityChecks.Update(updatedCheck);

        // Publish a updated message to the message system
        var responsibleOrganisations = await commonRepository.GetResponsibleOrganisations(updatedCheck.Easting, updatedCheck.Northing, ct);
        var fullFloodSource = await commonRepository.GetFullEligibilityFloodProblemSourceList(updatedCheck, ct);
        var updatedMessage = updatedCheck.ToMessageUpdated(responsibleOrganisations, fullFloodSource);

        await publishEndpoint.Publish(updatedMessage, ct);

        // Update the database with the eligibility check, message, flood impacts, and flood problems
        await context.SaveChangesAsync(ct);

        return updatedCheck;
    }
}

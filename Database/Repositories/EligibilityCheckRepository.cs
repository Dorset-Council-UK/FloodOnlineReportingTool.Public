using FloodOnlineReportingTool.Database.DbContexts;
using FloodOnlineReportingTool.Database.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FloodOnlineReportingTool.Database.Repositories;

public class EligibilityCheckRepository(ILogger<EligibilityCheckRepository> logger, PublicDbContext context, IPublishEndpoint publishEndpoint, ICommonRepository commonRepository) : IEligibilityCheckRepository
{
    public async Task<EligibilityCheck?> ReportedByUser(Guid userId, CancellationToken ct)
    {
        // TODO: Might need to check a status on the flood report
        return await context.FloodReports
            .AsNoTracking()
            .Where(o => o.ReportedByUserId == userId)
            .Select(o => o.EligibilityCheck)
            .FirstOrDefaultAsync(ct)
            .ConfigureAwait(false);
    }

    public async Task<EligibilityCheck?> ReportedByUser(Guid userId, Guid id, CancellationToken ct)
    {
        // TODO: Might need to check a status on the flood report
        return await context.FloodReports
            .AsNoTracking()
            .Where(o => o.ReportedByUserId == userId)
            .Select(o => o.EligibilityCheck)
            .FirstOrDefaultAsync(o => o != null && o.Id == id, ct)
            .ConfigureAwait(false);
    }

    public async Task<EligibilityCheck?> GetById(Guid id, CancellationToken ct)
    {
        logger.LogInformation("Getting eligibility check by id {Id}", id);

        // FloodProblems, and FloodImpacts will be auto included
        return await context.EligibilityChecks
            .FindAsync([id], ct)
            .ConfigureAwait(false);
    }

    public async Task<EligibilityCheck?> GetByReference(string reference, CancellationToken ct)
    {
        logger.LogInformation("Getting eligibility check by flood report reference {Reference}", reference);

        return await context.FloodReports
            .AsNoTracking()
            .Include(o => o.EligibilityCheck)
            .Where(o => o.Reference == reference)
            .Select(o => o.EligibilityCheck)
            .FirstOrDefaultAsync(ct)
            .ConfigureAwait(false);
    }

    public async Task<EligibilityCheck?> Update(Guid id, EligibilityCheckDto dto, CancellationToken ct)
    {
        logger.LogInformation("Update eligibility check {Id}", id);

        var existingCheck = await context.EligibilityChecks
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id, ct)
            .ConfigureAwait(false);

        if (existingCheck is null)
        {
            return null;
        }

        // Update the fields we choose
        var updatedCheck = existingCheck with
        {
            UpdatedUtc = DateTimeOffset.UtcNow,

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

            Residentials = [.. dto.Residentials.Select(floodImpactId => new EligibilityCheckResidential(existingCheck.Id, floodImpactId))],
            Commercials = [.. dto.Commercials.Select(floodImpactId => new EligibilityCheckCommercial(existingCheck.Id, floodImpactId))],
            Sources = [.. dto.Sources.Select(floodProblemId => new EligibilityCheckSource(existingCheck.Id, floodProblemId))],
        };
        context.EligibilityChecks.Update(updatedCheck);

        // Publish a updated message to the message system
        var responsibleOrganisations = await commonRepository
            .GetResponsibleOrganisations(updatedCheck.Easting, updatedCheck.Northing, ct)
            .ConfigureAwait(false);
        var updatedMessage = updatedCheck.ToMessageUpdated(responsibleOrganisations);

        await publishEndpoint.Publish(updatedMessage, ct).ConfigureAwait(false);

        // Update the database with the eligibility check, message, flood impacts, and flood problems
        await context.SaveChangesAsync(ct).ConfigureAwait(false);

        return updatedCheck;
    }

    public async Task<EligibilityCheck> UpdateForUser(Guid userId, Guid id, EligibilityCheckDto dto, CancellationToken ct)
    {
        var existingCheck = await context.FloodReports
            .AsNoTracking()
            .Include(o => o.EligibilityCheck)
            .Where(o => o.ReportedByUserId == userId)
            .Select(o => o.EligibilityCheck)
            .FirstOrDefaultAsync(o => o != null && o.Id == id, ct)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException("No eligiblity check found");

        // Update the fields we choose
        var updatedCheck = existingCheck with
        {
            UpdatedUtc = DateTimeOffset.UtcNow,

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

            Residentials = [.. dto.Residentials.Select(floodImpactId => new EligibilityCheckResidential(existingCheck.Id, floodImpactId))],
            Commercials = [.. dto.Commercials.Select(floodImpactId => new EligibilityCheckCommercial(existingCheck.Id, floodImpactId))],
            Sources = [.. dto.Sources.Select(floodProblemId => new EligibilityCheckSource(existingCheck.Id, floodProblemId))],
        };
        context.EligibilityChecks.Update(updatedCheck);

        // Publish a updated message to the message system
        var responsibleOrganisations = await commonRepository
            .GetResponsibleOrganisations(updatedCheck.Easting, updatedCheck.Northing, ct)
            .ConfigureAwait(false);
        var updatedMessage = updatedCheck.ToMessageUpdated( responsibleOrganisations);

        await publishEndpoint.Publish(updatedMessage, ct).ConfigureAwait(false);

        // Update the database with the eligibility check, message, flood impacts, and flood problems
        await context.SaveChangesAsync(ct).ConfigureAwait(false);

        return updatedCheck;
    }

    public async Task<EligibilityResult> CalculateEligibilityWithReference(string reference, CancellationToken ct)
    {
        logger.LogInformation("Calculating eligibility for flood report reference {Reference}", reference);

        var floodReport = await context.FloodReports
            .AsNoTracking()
            .Include(o => o.ContactRecords)
            .Include(o => o.EligibilityCheck)
            .Where(o => o.Reference == reference)
            .FirstOrDefaultAsync(ct)
            .ConfigureAwait(false);

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
                .GetResponsibleOrganisations(floodReport.EligibilityCheck.Easting, floodReport.EligibilityCheck.Northing, ct)
                .ConfigureAwait(false);

        return new EligibilityResult
        {
            HasContactInformation = floodReport.ContactRecords.Any(),
            FloodInvestigation = floodReport.EligibilityCheck.IsInternal() ? EligibilityOptions.Conditional : EligibilityOptions.None,
            ResponsibleOrganisations = responsibleOrganisations,

            // These don't have any logic yet
            IsEmergencyResponse = false,
            Section19Url = null,
            Section19 = EligibilityOptions.None,
            PropertyProtection = EligibilityOptions.None,
            GrantApplication = EligibilityOptions.None,
        };
    }
}

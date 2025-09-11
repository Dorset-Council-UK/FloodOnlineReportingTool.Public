using FloodOnlineReportingTool.Database.DbContexts;
using FloodOnlineReportingTool.Database.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;

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

        var eligibilityCheck = await context.EligibilityChecks
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id, ct)
            .ConfigureAwait(false);

        if (eligibilityCheck == null)
        {
            return null;
        }

        // Update the fields we choose
        var impactDuration = await GetImpactDurationHours(dto.OnGoing, dto.DurationKnownId, dto.ImpactDuration, ct).ConfigureAwait(false);
        var updatedCheck = eligibilityCheck with
        {
            UpdatedUtc = DateTimeOffset.UtcNow,

            IsAddress = dto.IsAddress,
            Uprn = dto.Uprn,
            Easting = dto.Easting,
            Northing = dto.Northing,
            LocationDesc = dto.LocationDesc,
            ImpactStart = dto.ImpactStart,
            ImpactDuration = impactDuration,
            OnGoing = dto.OnGoing,
            Uninhabitable = dto.Uninhabitable == true,
            VulnerablePeopleId = dto.VulnerablePeopleId,
            VulnerableCount = dto.VulnerableCount,
            Residentials = [.. dto.Residentials.Select(floodImpactId => new EligibilityCheckResidential(id, floodImpactId))],
            Commercials = [.. dto.Commercials.Select(floodImpactId => new EligibilityCheckCommercial(id, floodImpactId))],
            Sources = [.. dto.Sources.Select(floodProblemId => new EligibilityCheckSource(id, floodProblemId))],
        };
        context.EligibilityChecks.Update(updatedCheck);

        // Publish a updated message to the message system
        var responsibleOrganisations = await commonRepository
            .GetResponsibleOrganisations(updatedCheck.Easting, updatedCheck.Northing, ct)
            .ConfigureAwait(false);
        IList<FloodProblem> fullFloodSource = context.FloodProblems.Where(f =>
                updatedCheck.Sources.Select(s => new EligibilityCheckSourceDto(s.EligibilityCheckId, s.FloodProblemId)).Concat(updatedCheck.SecondarySources.Select(r => new EligibilityCheckSourceDto(r.EligibilityCheckId, r.FloodProblemId)))
                .Select(s => s.FloodProblemId).ToList().Contains(f.Id)
            ).ToList();
        var updatedMessage = updatedCheck.ToMessageUpdated(responsibleOrganisations, fullFloodSource);

        await publishEndpoint.Publish(updatedMessage, ct).ConfigureAwait(false);

        // Update the database with the eligibility check, message, flood impacts, and flood problems
        await context.SaveChangesAsync(ct).ConfigureAwait(false);

        return updatedCheck;
    }

    public async Task<EligibilityCheck> UpdateForUser(Guid userId, Guid id, EligibilityCheckDto dto, CancellationToken ct)
    {
        var eligibilityCheck = await context.FloodReports
            .AsNoTracking()
            .Include(o => o.EligibilityCheck)
            .Where(o => o.ReportedByUserId == userId)
            .Select(o => o.EligibilityCheck)
            .FirstOrDefaultAsync(o => o != null && o.Id == id, ct)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException("No eligiblity check found");

        // Update the fields we choose
        var impactDuration = await GetImpactDurationHours(dto.OnGoing, dto.DurationKnownId, dto.ImpactDuration, ct).ConfigureAwait(false);
        var updatedCheck = eligibilityCheck with
        {
            UpdatedUtc = DateTimeOffset.UtcNow,

            IsAddress = dto.IsAddress,
            Uprn = dto.Uprn,
            Easting = dto.Easting,
            Northing = dto.Northing,
            LocationDesc = dto.LocationDesc,
            ImpactStart = dto.ImpactStart,
            ImpactDuration = impactDuration,
            OnGoing = dto.OnGoing,
            Uninhabitable = dto.Uninhabitable == true,
            VulnerablePeopleId = dto.VulnerablePeopleId,
            VulnerableCount = dto.VulnerableCount,
            Residentials = [.. dto.Residentials.Select(floodImpactId => new EligibilityCheckResidential(id, floodImpactId))],
            Commercials = [.. dto.Commercials.Select(floodImpactId => new EligibilityCheckCommercial(id, floodImpactId))],
            Sources = [.. dto.Sources.Select(floodProblemId => new EligibilityCheckSource(id, floodProblemId))],
        };
        context.EligibilityChecks.Update(updatedCheck);

        // Publish a updated message to the message system
        var responsibleOrganisations = await commonRepository
            .GetResponsibleOrganisations(updatedCheck.Easting, updatedCheck.Northing, ct)
            .ConfigureAwait(false);
        IList<FloodProblem> fullFloodSource = context.FloodProblems.Where(f =>
                updatedCheck.Sources.Select(s => new EligibilityCheckSourceDto(s.EligibilityCheckId, s.FloodProblemId)).Concat(updatedCheck.SecondarySources.Select(r => new EligibilityCheckSourceDto(r.EligibilityCheckId, r.FloodProblemId)))
                .Select(s => s.FloodProblemId).ToList().Contains(f.Id)
            ).ToList();
        var updatedMessage = updatedCheck.ToMessageUpdated(responsibleOrganisations, fullFloodSource);

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

    /// <summary>
    ///     <para>Calculates the impact duration hours.</para>
    ///     <para>If the flood is still happening this will be zero.</para>
    ///     <para>If the flood duration is known, it will return the hours provided by the user.</para>
    ///     <para>Otherwise, it will try to get the duration hours from the flood problem in the database.</para>
    /// </summary>
    /// <remarks>This logic is currently in 2 places the FloodReportRepository and the EligibilityCheckRespository.</remarks>
    internal async Task<int> GetImpactDurationHours(bool isOngoing, Guid? durationKnownId, int? impactDurationHours, CancellationToken ct)
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
        if (durationKnownId == Models.FloodProblemIds.FloodDurationIds.DurationKnown)
        {
            logger.LogInformation("Impact duration is known, using provided impact duration hours.");
            return impactDurationHours ?? 0;
        }

        // Get the duration from the flood problem
        var floodProblem = await context.FloodProblems
            .FindAsync([durationKnownId], ct)
            .ConfigureAwait(false);

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

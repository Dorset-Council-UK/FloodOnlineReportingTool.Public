using FloodOnlineReportingTool.Database.DbContexts;
using FloodOnlineReportingTool.Database.Models.Eligibility;
using FloodOnlineReportingTool.Database.Models.Flood;
using FloodOnlineReportingTool.Database.Models.Responsibilities;
using FloodOnlineReportingTool.Database.Models.Status;
using FloodOnlineReportingTool.Database.Services;
using Microsoft.EntityFrameworkCore;

namespace FloodOnlineReportingTool.Database.Repositories;

public class CommonRepository(IDbContextFactory<PublicDbContext> contextFactory, IBoundaryApiClient boundaryApiClient) : ICommonRepository
{
    public async Task<FloodImpact?> GetFloodImpact(Guid id, CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);
        return await context.FloodImpacts.FindAsync([id], ct);
    }

    public async Task<IList<FloodImpact>> GetFloodImpactsByCategory(string category, CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);
        return await context.FloodImpacts
            .AsNoTracking()
            .Where(fi => fi.Category == category)
            .OrderBy(fi => fi.OptionOrder)
            .ToListAsync(ct);
    }

    public async Task<IList<FloodImpact>> GetFloodImpactsByCategories(string[] categories, CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);
        return await context.FloodImpacts
            .AsNoTracking()
            .Where(fi => categories.Contains(fi.Category))
            .OrderBy(fi=> fi.OptionOrder)
            .ToListAsync(ct);
    }

    public async Task<FloodProblem?> GetFloodProblem(Guid id, CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);
        return await context.FloodProblems.FindAsync([id], ct);
    }

    public async Task<FloodProblem?> GetFloodProblemByCategory(string category, Guid id, CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);
        return await context.FloodProblems
            .AsNoTracking()
            .FirstOrDefaultAsync(fp => fp.Category == category && fp.Id == id, ct);
    }

    public async Task<IList<FloodProblem>> GetFloodProblemsByCategory(string category, CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);
        return await context.FloodProblems
            .AsNoTracking()
            .Where(fp => fp.Category == category)
            .OrderBy(fp => fp.OptionOrder)
            .ToListAsync(ct);
    }

    public async Task<IList<FloodProblem>> GetFloodProblemsByCategories(string[] categories, CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);
        return await context.FloodProblems
            .AsNoTracking()
            .Where(fp => categories.Contains(fp.Category))
            .OrderBy(fp => fp.OptionOrder)
            .ToListAsync(ct);
    }

    public async Task<IList<FloodProblem>> FilterFloodProblemsByCategories(string[] categories, IList<FloodProblem> problemList, CancellationToken ct)
    {
        var filterHashSet = (await GetFloodProblemsByCategories(categories, ct))
            .Select(fp => fp.Id)
            .ToHashSet();
        return [.. problemList.Where(p => filterHashSet.Contains(p.Id))];
    }

    public async Task<IList<FloodProblem>> GetFloodProblemsForCauses(EligibilityCheck eligibilityCheck, CancellationToken ct)
    {
        IList<Guid> primaryCauses = [.. eligibilityCheck.Causes.Select(r => r.FloodProblemId)];
        IList<Guid> secondaryCauses = [.. eligibilityCheck.SecondaryCauses.Select(r => r.FloodProblemId)];

        var allCauses = primaryCauses.Concat(secondaryCauses);

        await using var context = await contextFactory.CreateDbContextAsync(ct);
        IList<FloodProblem> floodProblems = await context.FloodProblems
            .Where(fp => allCauses.Contains(fp.Id))
            .ToListAsync(ct);

        return floodProblems;
    }

    public async Task<IList<FloodMitigation>> GetFloodMitigationsByCategory(string category, CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);
        return await context.FloodMitigations
            .AsNoTracking()
            .Where(fm => fm.Category == category)
            .OrderBy(fm => fm.OptionOrder)
            .ToListAsync(ct);
    }

    public async Task<IList<FloodMitigation>> GetFloodMitigationsByCategories(string[] categories, CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);
        return await context.FloodMitigations
            .AsNoTracking()
            .Where(fm => categories.Contains(fm.Category))
            .OrderBy(fm => fm.OptionOrder)
            .ToListAsync(ct);
    }

    public async Task<RecordStatus?> GetRecordStatus(Guid id, CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);
        return await context.RecordStatuses.FindAsync([id], ct);
    }

    public async Task<IList<RecordStatus>> GetRecordStatusesByCategory(string category, CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);
        return await context.RecordStatuses
            .AsNoTracking()
            .Where(rs => rs.Category == category)
            .OrderBy(rs => rs.Order)
            .ToListAsync(ct);
    }

    public async Task<IList<RecordStatus>> GetRecordStatusesByCategories(string[] categories, CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);
        return await context.RecordStatuses
            .AsNoTracking()
            .Where(rs => categories.Contains(rs.Category))
            .OrderBy(rs => rs.Order)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Get the organisations responsible based on the location of the flood.
    /// </summary>
    public async Task<IList<Organisation>> GetResponsibleOrganisations(double? easting, double? northing, CancellationToken ct)
    {
        if (easting is null || northing is null)
        {
            return [];
        }

        // Get county admin unit ID for the given easting/northing
        var adminUnitIds = await boundaryApiClient.GetCountyAdminUnitIds(easting.Value, northing.Value, ct);

        if (adminUnitIds.Count == 0)
        {
            return [];
        }

        // Get all the responsible organisations
        // By starting with the FloodResponsibilities table instead of the Organisations table
        // Entity Framework will generate 2 efficient INNER JOIN's in the SQL because of the configured auto includes
        // CalculateEligibility() needs the flood authority for each organisation, which is then used in the Confirmation view model
        await using var context = await contextFactory.CreateDbContextAsync(ct);
        var organisations = await context.FloodResponsibilities
            .AsNoTracking()
            .Where(fr => adminUnitIds.Contains(fr.AdminUnitId))
            .Select(fr => fr.Organisation)
            .ToListAsync(ct);

        return organisations;
    }
}

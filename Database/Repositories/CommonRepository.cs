using FloodOnlineReportingTool.Contracts.Shared;
using FloodOnlineReportingTool.Database.DbContexts;
using FloodOnlineReportingTool.Database.Models;
using MassTransit.Initializers;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace FloodOnlineReportingTool.Database.Repositories;

public class CommonRepository(PublicDbContext context, BoundariesDbContext boundariesDb) : ICommonRepository
{
    private const string SqlAllCounties = @"SELECT name, area_description, admin_unit_id FROM dc_boundaries.uk_county WHERE public.ST_Contains(geom, public.ST_SetSRID(public.ST_MakePoint({0}, {1}), 27700)) ORDER BY name";

    public async Task<HashSet<Guid>> GetClassHash(Type T, CancellationToken ct)
    {
        // HashSet for fast lookup
        return T.GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.FieldType == typeof(Guid))
            .Select(f => (Guid)f.GetValue(null))
            .ToHashSet();
    }

    public async Task<FloodImpact?> GetFloodImpact(Guid id, CancellationToken ct)
    {
        return await context.FloodImpacts
            .FindAsync([id], ct)
            .ConfigureAwait(false);
    }

    public async Task<IList<FloodImpact>> GetFloodImpactsByCategory(string category, CancellationToken ct)
    {
        return await context.FloodImpacts
            .AsNoTracking()
            .Where(o => o.Category == category)
            .OrderBy(o => o.OptionOrder)
            .ToListAsync(ct)
            .ConfigureAwait(false);
    }

    public async Task<FloodProblem?> GetFloodProblemByCategory(string category, Guid id, CancellationToken ct)
    {
        return await context.FloodProblems
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Category == category && o.Id == id, ct)
            .ConfigureAwait(false);
    }

    public async Task<IList<FloodProblem>> GetFloodProblemsByCategory(string category, CancellationToken ct)
    {
        return await context.FloodProblems
            .AsNoTracking()
            .Where(o => o.Category == category)
            .OrderBy(o => o.OptionOrder)
            .ToListAsync(ct)
            .ConfigureAwait(false);
    }

    public async Task<IList<FloodProblem>> GetFloodProblemsByCategories(string[] categories, CancellationToken ct)
    {
        return await context.FloodProblems
            .AsNoTracking()
            .Where(o => categories.Contains(o.Category))
            .OrderBy(o => o.OptionOrder)
            .ToListAsync(ct)
            .ConfigureAwait(false);
    }

    public async Task<IList<FloodProblem>> FilterFloodProblemsByCategories(string[] categories, IList<FloodProblem> problemList, CancellationToken ct)
    {
        var filterHashSet = (await GetFloodProblemsByCategories(categories, ct).ConfigureAwait(false))
            .Select(f => f.Id)
            .ToHashSet();
        return problemList.Where(p => filterHashSet.Contains(p.Id)).ToList();
    }

    public async Task<IList<FloodMitigation>> GetFloodMitigationsByCategory(string category, CancellationToken ct)
    {
        return await context.FloodMitigations
            .AsNoTracking()
            .Where(o => o.Category == category)
            .OrderBy(o => o.OptionOrder)
            .ToListAsync(ct)
            .ConfigureAwait(false);
    }

    public async Task<IList<FloodMitigation>> GetFloodMitigationsByCategories(string[] categories, CancellationToken ct)
    {
        return await context.FloodMitigations
            .AsNoTracking()
            .Where(o => categories.Contains(o.Category))
            .OrderBy(o => o.OptionOrder)
            .ToListAsync(ct)
            .ConfigureAwait(false);
    }

    public async Task<RecordStatus?> GetRecordStatus(Guid id, CancellationToken ct)
    {
        return await context.RecordStatuses
            .FindAsync([id], ct)
            .ConfigureAwait(false);
    }

    public async Task<IList<RecordStatus>> GetRecordStatusesByCategory(string category, CancellationToken ct)
    {
        return await context.RecordStatuses
            .AsNoTracking()
            .Where(o => o.Category == category)
            .OrderBy(o => o.Order)
            .ToListAsync(ct)
            .ConfigureAwait(false);
    }

    public async Task<IList<RecordStatus>> GetRecordStatusesByCategories(string[] categories, CancellationToken ct)
    {
        return await context.RecordStatuses
            .AsNoTracking()
            .Where(o => categories.Contains(o.Category))
            .OrderBy(o => o.Order)
            .ToListAsync(ct)
            .ConfigureAwait(false);
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

        // Get all the counties which intersect the flood location
        var adminUnitIds = await boundariesDb.Counties
            .FromSqlRaw(SqlAllCounties, easting, northing)
            .Select(c => c.AdminUnitId)
            .ToListAsync(ct)
            .ConfigureAwait(false);

        if (adminUnitIds is null || adminUnitIds.Count == 0)
        {
            return [];
        }

        // Get all the responsible organisations
        // By starting with the FloodResponsibilities table instead of the Organisations table
        // Entity Framework will generate 2 efficient INNER JOIN's in the SQL because of the configured auto includes
        // CalculateEligibility() needs the flood authority for each organisation, which is then used in the Confirmation view model
        var organisations = await context.FloodResponsibilities
            .AsNoTracking()
            .Where(fr => adminUnitIds.Contains(fr.AdminUnitId))
            .Select(fr => fr.Organisation)
            .ToListAsync(ct)
            .ConfigureAwait(false);

        return organisations;
    }
}

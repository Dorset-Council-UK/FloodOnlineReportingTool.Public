using FloodOnlineReportingTool.Database.Models.Eligibility;
using FloodOnlineReportingTool.Database.Models.Flood;
using FloodOnlineReportingTool.Database.Models.Responsibilities;
using FloodOnlineReportingTool.Database.Models.Status;

namespace FloodOnlineReportingTool.Database.Repositories;

public interface ICommonRepository
{
    Task<FloodImpact?> GetFloodImpact(Guid id, CancellationToken ct);
    Task<IList<FloodImpact>> GetFloodImpactsByCategory(string category, CancellationToken ct);
    Task<IList<FloodImpact>> GetFloodImpactsByCategories(string[] categories, CancellationToken ct);
    Task<FloodProblem?> GetFloodProblem(Guid id, CancellationToken ct);
    Task<FloodProblem?> GetFloodProblemByCategory(string category, Guid id, CancellationToken ct);
    Task<IList<FloodProblem>> GetFloodProblemsByCategory(string category, CancellationToken ct);
    Task<IList<FloodProblem>> GetFloodProblemsByCategories(string[] categories, CancellationToken ct);
    Task<IList<FloodProblem>> FilterFloodProblemsByCategories(string[] categories, IList<FloodProblem> problemList, CancellationToken ct);
    Task<IList<FloodProblem>> GetFullEligibilityFloodProblemSourceList(EligibilityCheck eligibilityCheck, CancellationToken ct);
    Task<IList<FloodMitigation>> GetFloodMitigationsByCategory(string category, CancellationToken ct);
    Task<IList<FloodMitigation>> GetFloodMitigationsByCategories(string[] categories, CancellationToken ct);
    Task<RecordStatus?> GetRecordStatus(Guid id, CancellationToken ct);
    Task<IList<RecordStatus>> GetRecordStatusesByCategory(string category, CancellationToken ct);
    Task<IList<RecordStatus>> GetRecordStatusesByCategories(string[] categories, CancellationToken ct);
    Task<IList<Organisation>> GetResponsibleOrganisations(double? easting, double? northing, CancellationToken ct);
}

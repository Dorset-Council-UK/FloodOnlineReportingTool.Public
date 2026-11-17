namespace FloodOnlineReportingTool.Database.Services;

public interface IBoundaryApiClient
{
    Task<IReadOnlyCollection<int>> GetCountyAdminUnitIds(double easting, double northing, CancellationToken ct);
}
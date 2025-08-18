using FloodOnlineReportingTool.Database.Models;

namespace FloodOnlineReportingTool.Database.Repositories;

public interface ISearchRepository
{
    Task<IList<ApiAddress>> AddressSearch(string postcode, Uri? referer, CancellationToken ct);
    Task IsAddressSearchAvailable(Uri? referer, CancellationToken ct);
    Task<HttpResponseMessage?> GetNearestAddressResponse(double easting, double northing, Uri? referer, CancellationToken ct);
    Task IsNearestAddressAvailable(Uri? referer, CancellationToken ct);
}

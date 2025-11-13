using FloodOnlineReportingTool.Database.Models.API;

namespace FloodOnlineReportingTool.Database.Repositories;

public interface ISearchRepository
{
    Task<IList<ApiAddress>> AddressSearch(string postcode, SearchAreaOptions searchArea, Uri? referer, CancellationToken ct);
    Task IsAddressSearchAvailable(Uri? referer, SearchAreaOptions searchArea, CancellationToken ct);
    Task<HttpResponseMessage?> GetNearestAddressResponse(double easting, double northing, SearchAreaOptions searchArea, Uri? referer, CancellationToken ct);
    Task IsNearestAddressAvailable(Uri? referer, SearchAreaOptions searchArea, CancellationToken ct);
}

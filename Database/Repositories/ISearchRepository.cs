using FloodOnlineReportingTool.Database.Models;
using static FloodOnlineReportingTool.Database.Repositories.SearchRepository;

namespace FloodOnlineReportingTool.Database.Repositories;

public interface ISearchRepository
{
    Task<IList<ApiAddress>> AddressSearch(string postcode, SearchArea searchArea, Uri? referer, CancellationToken ct);
    Task IsAddressSearchAvailable(Uri? referer, SearchArea searchArea, CancellationToken ct);
    Task<HttpResponseMessage?> GetNearestAddressResponse(double easting, double northing, SearchArea searchArea, Uri? referer, CancellationToken ct);
    Task IsNearestAddressAvailable(Uri? referer, SearchArea searchArea, CancellationToken ct);
}

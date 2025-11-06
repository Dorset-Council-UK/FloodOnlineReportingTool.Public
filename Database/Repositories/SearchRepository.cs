using FloodOnlineReportingTool.Database.Exceptions;
using FloodOnlineReportingTool.Database.Models.API;
using FloodOnlineReportingTool.Database.Settings;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;

namespace FloodOnlineReportingTool.Database.Repositories;

public class SearchRepository : ISearchRepository
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly GISSettings _settings;
    private readonly JsonSerializerOptions _jsonOptions;

    public SearchRepository(IHttpClientFactory httpClientFactory, IOptions<GISSettings> settings)
    {
        _httpClientFactory = httpClientFactory;
        _settings = settings.Value;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };
    }

    /// <summary>
    /// Create a search URI using the search term, URL and the API key from configuration.
    /// </summary>
    /// <param name="searchTerm">The search term, for instance postcode or UPRN</param>
    /// <exception cref="ConfigurationMissingException">If the search URI or API key are not set in the settings</exception>
    private Uri CreateAddressSearchUri(string searchTerm, SearchAreaOptions searchArea)
    {
        // Throw an error if the search URI or API key are not set
        var searchUri = _settings.AddressSearchUrl
            ?? throw new ConfigurationMissingException("Missing configuration setting: The search URL is not set in the settings. Under GIS > AddressSearchUrl");

        if (_settings.ApiKey is null)
        {
            throw new ConfigurationMissingException("Missing configuration setting: The GIS API key not set in the settings. Under GIS > ApiKey");
        }

        // Encode the search term
        var encodedSearchTerm = Uri.EscapeDataString(searchTerm);

        // Construct the search query using the Uri Builder, the search Uri and the API key are in the settings
        var builder = new UriBuilder(searchUri)
        {
            Query = $"query={encodedSearchTerm}&searchArea=" + searchArea.ToString(),
        };
        return builder.Uri;
    }

    /// <summary>
    /// Create a nearest addresses URI using easting and norhing, URL and the API key from configuration.
    /// </summary>
    /// <param name="easting" />
    /// <param name="northing" />
    /// <exception cref="ConfigurationMissingException">If the nearest addresses URI or API key are not set in the settings</exception>
    private Uri CreateNearestAddressUri(double easting, double northing, SearchAreaOptions searchArea)
    {
        // Throw an error if the search URI or API key are not set
        var nearestAddressesUri = _settings.NearestAddressesUrl
            ?? throw new ConfigurationMissingException("Missing configuration setting: The nearest addresses URL is not set in the settings. Under GIS > NearestAddressesUrl");

        if (_settings.ApiKey is null)
        {
            throw new ConfigurationMissingException("Missing configuration setting: The GIS API key not set in the settings. Under GIS > ApiKey");
        }

        // Construct the nearest addresses query using the Uri Builder, the nearest addresses Uri and the API key are in the settings
        var builder = new UriBuilder(nearestAddressesUri)
        {
            Query = string.Create(CultureInfo.InvariantCulture, $"x={easting}&y={northing}&maxResults=1&epsg=27700&searchArea={searchArea.ToString()}"),
        };
        return builder.Uri;
    }

    private async Task<HttpResponseMessage> GetResponse(Uri uri, Uri? referer, CancellationToken ct)
    {
        using var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("User-Agent", "HttpClientFactory-FortApplication");
        if (referer is not null)
        {
            client.DefaultRequestHeaders.Add("Referer", referer.ToString());
        }
        client.DefaultRequestHeaders.Add("X-API-Key", _settings.ApiKey);

        return await client.GetAsync(uri, ct);
    }

    public async Task<IList<ApiAddress>> AddressSearch(string postcode, SearchAreaOptions searchArea, Uri? referer, CancellationToken ct)
    {
        var response = await GetResponse(CreateAddressSearchUri(postcode, searchArea), referer, ct);

        if (response.IsSuccessStatusCode)
        {
            var advancedSearch = await response.Content.ReadFromJsonAsync<List<ApiAddress>>(_jsonOptions, ct);

            if (advancedSearch is not null)
            {
                return advancedSearch;
            }
        }

        return [];
    }

    /// <summary>
    /// Get the nearest address response to the given easting and northing.
    /// </summary>
    public async Task<HttpResponseMessage?> GetNearestAddressResponse(double easting, double northing, SearchAreaOptions searchArea, Uri? referer, CancellationToken ct)
    {
        return await GetResponse(CreateNearestAddressUri(easting, northing, searchArea), referer, ct);
    }

    /// <summary>
    /// Health check to see if the address search is available.
    /// </summary>
    public async Task IsAddressSearchAvailable(Uri? referer, SearchAreaOptions searchArea, CancellationToken ct)
    {
        var response = await GetResponse(CreateAddressSearchUri("", searchArea), referer, ct);

        // Expecting a 400 response, throw an exception for anything else
        if (response.StatusCode != System.Net.HttpStatusCode.BadRequest)
        {
            throw new Exception($"Address search is not available. Status code: {response.StatusCode}");
        }
    }

    /// <summary>
    /// Health check to see if the nearest address is available.
    /// </summary>
    public async Task IsNearestAddressAvailable(Uri? referer, SearchAreaOptions searchArea, CancellationToken ct)
    {
        var response = await GetResponse(CreateNearestAddressUri(0, 0, searchArea), referer, ct);

        // Expecting a 400 response, throw an exception for anything else
        if (response.StatusCode != System.Net.HttpStatusCode.BadRequest)
        {
            throw new Exception($"Nearest address is not available. Status code: {response.StatusCode}");
        }
    }
}

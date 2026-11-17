using FloodOnlineReportingTool.Database.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FloodOnlineReportingTool.Database.Services;

public class BoundaryApiClient(
    HttpClient httpClient,
    IOptions<BoundaryApiOptions> options,
    ILogger<BoundaryApiClient> logger) : IBoundaryApiClient
{
    public async Task<IReadOnlyCollection<int>> GetCountyAdminUnitIds(double easting, double northing, CancellationToken ct)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(easting, 700_000);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(northing, 1_300_000);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(easting);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(northing);

        var relativePath = options.Value.GetCountyPathTemplate
            .Replace("{x}", easting.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal)
            .Replace("{y}", northing.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal);

        logger.LogInformation("Checking downstream boundary API for county admin unit IDs");
        using var response = await httpClient.GetAsync(relativePath, ct);
        response.EnsureSuccessStatusCode();

        var collection = await response.Content.ReadFromJsonAsync<CountyFeatureCollection>(ct)
            ?? throw new JsonException("The boundary API returned a null feature collection.");
        if (!string.Equals(collection.Type, "FeatureCollection", StringComparison.Ordinal) || collection.Features is null)
        {
            throw new JsonException("The boundary API must return a GeoJSON FeatureCollection with a features array.");
        }

        var adminUnitIds = new HashSet<int>();
        foreach (var feature in collection.Features)
        {
            if (feature is null || !string.Equals(feature.Type, "Feature", StringComparison.Ordinal)
                || feature.Properties is null || feature.Properties.AdminUnitId <= 0)
            {
                throw new JsonException("Each boundary feature must contain properties with a positive admin_unit_id.");
            }

            adminUnitIds.Add(feature.Properties.AdminUnitId);
        }

        logger.LogInformation("Received {CountyCount} county admin unit IDs from the boundary API", adminUnitIds.Count);
        return adminUnitIds;
    }

    private sealed record CountyFeatureCollection(
        string? Type,
        CountyFeature?[]? Features);

    private sealed record CountyFeature(
        string? Type,
        CountyProperties? Properties);

    private sealed record CountyProperties(
        [property: JsonPropertyName("admin_unit_id")] int AdminUnitId);
}
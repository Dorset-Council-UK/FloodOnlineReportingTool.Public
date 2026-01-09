using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Caching.Hybrid;
using System.Globalization;

namespace FloodOnlineReportingTool.Public.State;

public class FloodReportCreateState(HybridCache cache)
{
    private readonly IEnumerable<string> _tags = [ "flood-report", "create", "temporary" ];
    private const string CacheKeyFormat = "flood-report:create:{0}";

    [PersistentState]
    public string? Postcode { get; set; }

    [PersistentState]
    public string? PrimaryClassification { get; set; }

    [PersistentState]
    public string? SecondaryClassification { get; set; }

    [PersistentState]
    public Guid? PropertyType { get; set; }

    [PersistentState]
    public string? TemporaryPostcode { get; set; }

    private void CopyFrom(FloodReportCreateState other)
    {
        Postcode = other.Postcode;
        PrimaryClassification = other.PrimaryClassification;
        SecondaryClassification = other.SecondaryClassification;
        PropertyType = other.PropertyType;
        TemporaryPostcode = other.TemporaryPostcode;
    }

    public async Task CopyFromCache(long cacheKey, CancellationToken cancellationToken = default)
    {
        var key = string.Format(CultureInfo.InvariantCulture, CacheKeyFormat, cacheKey);
        var cachedState = await cache.GetOrCreateAsync(
            key,
            async token => new FloodReportCreateState(cache),
            options: null,
            tags: _tags,
            cancellationToken: cancellationToken
        );

        CopyFrom(cachedState);
    }

    public async Task SaveToCache(long cacheKey, CancellationToken cancellationToken = default)
    {
        var key = string.Format(CultureInfo.InvariantCulture, CacheKeyFormat, cacheKey);
        await cache.SetAsync(key, this, tags: _tags, cancellationToken: cancellationToken);
    }
}

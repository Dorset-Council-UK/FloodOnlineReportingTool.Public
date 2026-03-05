using FloodOnlineReportingTool.Database.DbContexts;
using FloodOnlineReportingTool.Public.Health;

namespace Microsoft.AspNetCore.Builder;

internal static class HealthCheckExtensions
{

    private static readonly IEnumerable<string> tagsDatabase = ["database"];
    private static readonly IEnumerable<string> tagsServiceBusSubscription = ["messaging", "azure", "service bus", "subscription"];
    private static readonly IEnumerable<string> tagsAddressSearchAPI = ["api", "address", "search"];
    private static readonly IEnumerable<string> tagsAddressNearestAPI = ["api", "address", "nearest"];

    extension(IHostApplicationBuilder builder)
    {
        internal IHostApplicationBuilder AddFloodReportingHealthChecks()
        {
            builder.Services
                .AddHealthChecks()
                .AddDbContextCheck<PublicDbContext>(tags: tagsDatabase)
                .AddDbContextCheck<UserDbContext>(tags: tagsDatabase)
                .AddDbContextCheck<BoundariesDbContext>(tags: tagsDatabase)
                .AddCheck<ApiAddressSearchHealthCheck>("ApiAddressSearch", tags: tagsAddressSearchAPI)
                .AddCheck<ApiNearestAddressesHealthCheck>("ApiNearestAddresses", tags: tagsAddressNearestAPI);

            return builder;
        }
    }
}

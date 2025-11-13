using FloodOnlineReportingTool.Database.DbContexts;
using FloodOnlineReportingTool.Public.Health;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.AspNetCore.Builder;
#pragma warning restore IDE0130 // Namespace does not match folder structure

internal static class HealthCheckExtensions
{
    private static readonly IEnumerable<string> tagsDatabase = ["database"];
    private static readonly IEnumerable<string> tagsServiceBusSubscription = ["messaging", "azure", "service bus", "subscription"];
    private static readonly IEnumerable<string> tagsAddressSearchAPI = ["api", "address", "search"];
    private static readonly IEnumerable<string> tagsAddressNearestAPI = ["api", "address", "nearest"];
    private static readonly IEnumerable<string> tagsLive = ["live"];

    internal static IServiceCollection AddFloodReportingHealthChecks(this IServiceCollection services)
    {
        var builder = services
            .AddHealthChecks()
            .AddDbContextCheck<PublicDbContext>(tags: tagsDatabase)
            .AddDbContextCheck<UserDbContext>(tags: tagsDatabase)
            .AddDbContextCheck<BoundariesDbContext>(tags: tagsDatabase)
            .AddCheck<ApiAdvancedSearchHealthCheck>("ApiAdvancedSearch", tags: tagsAddressSearchAPI)
            .AddCheck<ApiNearestAddressesHealthCheck>("ApiNearestAddresses", tags: tagsAddressNearestAPI)
            // Add a default liveness check to ensure app is responsive
            .AddCheck("self", () => HealthCheckResult.Healthy(), tagsLive);

        return services;
    }

    internal static void MapFloodReportingHealthChecks(this WebApplication app)
    {
        app.MapHealthChecks("/health", new HealthCheckOptions()
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
        });

        app.MapHealthChecks("/alive", new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("live"),
        });
    }
}

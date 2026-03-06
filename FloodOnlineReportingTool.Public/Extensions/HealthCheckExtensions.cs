using FloodOnlineReportingTool.Public.Health;

namespace Microsoft.AspNetCore.Builder;

internal static class HealthCheckExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        internal IHostApplicationBuilder AddFloodReportingHealthChecks()
        {
            builder.Services
                .AddHealthChecks()
                .AddCheck<ApiAddressSearchHealthCheck>("ApiAddressSearch", tags: ["api", "address", "search"])
                .AddCheck<ApiNearestAddressesHealthCheck>("ApiNearestAddresses", tags: ["api", "address", "nearest"]);

            return builder;
        }
    }
}

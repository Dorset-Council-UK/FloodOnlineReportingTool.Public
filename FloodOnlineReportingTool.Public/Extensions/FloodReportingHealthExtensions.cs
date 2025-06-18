using FloodOnlineReportingTool.DataAccess.DbContexts;
using FloodOnlineReportingTool.Public.Health;
using FloodOnlineReportingTool.Public.Settings;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using RabbitMQ.Client;

namespace Microsoft.AspNetCore.Builder;

internal static class FloodReportingHealthExtensions
{
    private static readonly IEnumerable<string> tagsDatabase = ["database"];
    private static readonly IEnumerable<string> tagsRabbitMQ = ["rabbitmq"];
    private static readonly IEnumerable<string> tagsAddressSearchAPI = ["api", "address", "search"];
    private static readonly IEnumerable<string> tagsAddressNearestAPI = ["api", "address", "nearest"];
    private static readonly IEnumerable<string> tagsLive = ["live"];

    public static IServiceCollection AddFloodReportingHealthChecks(this IServiceCollection services, IConfigurationSection? rabbitMqSection)
    {
        services
            .AddHealthChecks()
            .AddDbContextCheck<FORTDbContext>(tags: tagsDatabase)
            .AddDbContextCheck<BoundariesDbContext>(tags: tagsDatabase)
            .AddCheck<ApiAdvancedSearchHealthCheck>("ApiAdvancedSearch", tags: tagsAddressSearchAPI)
            .AddCheck<ApiNearestAddressesHealthCheck>("ApiNearestAddresses", tags: tagsAddressNearestAPI)
            // Add a default liveness check to ensure app is responsive
            .AddCheck("self", () => HealthCheckResult.Healthy(), tagsLive);

        // Messaging options and health checks
        if (rabbitMqSection is not null)
        {
            var rabbitMqSettings = rabbitMqSection.Get<RabbitMqSettings>();
            if (rabbitMqSettings is not null && rabbitMqSettings.Enabled)
            {
                // Add a long lived RabbitMQ connection
                services.AddSingleton<IConnection>(sp =>
                {
                    var factory = new ConnectionFactory
                    {
                        Uri = rabbitMqSettings.Host,
                        UserName = rabbitMqSettings.Username,
                        Password = rabbitMqSettings.Password,
                    };
                    return factory.CreateConnectionAsync().GetAwaiter().GetResult();
                });

                // Add RabbitMQ health checks
                services
                    .AddHealthChecks()
                    .AddRabbitMQ(tags: tagsRabbitMQ);
            }
        }

        return services;
    }

    public static void MapFloodReportingHealthChecks(this WebApplication app)
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

using FloodOnlineReportingTool.DataAccess.DbContexts;
using FloodOnlineReportingTool.Public.Health;
using FloodOnlineReportingTool.Public.Settings;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.AspNetCore.Builder;

internal static class HealthCheckExtensions
{
    private static readonly IEnumerable<string> tagsDatabase = ["database"];
    private static readonly IEnumerable<string> tagsServiceBusQueue = ["messaging", "azure", "service bus", "queue"];
    private static readonly IEnumerable<string> tagsServiceBusThreshold = ["messaging", "azure", "service bus", "threshold"];
    private static readonly IEnumerable<string> tagsServiceBusSubscription = ["messaging", "azure", "service bus", "subscription"];
    private static readonly IEnumerable<string> tagsServiceBusTopic = ["messaging", "azure", "service bus", "topic"];
    private static readonly IEnumerable<string> tagsAddressSearchAPI = ["api", "address", "search"];
    private static readonly IEnumerable<string> tagsAddressNearestAPI = ["api", "address", "nearest"];
    private static readonly IEnumerable<string> tagsLive = ["live"];

    public static IServiceCollection AddFloodReportingHealthChecks(this IServiceCollection services, MessagingSettings messagingSettings)
    {
        var builder = services
            .AddHealthChecks()
            .AddDbContextCheck<FORTDbContext>(tags: tagsDatabase)
            .AddDbContextCheck<UserDbContext>(tags: tagsDatabase)
            .AddDbContextCheck<BoundariesDbContext>(tags: tagsDatabase)
            .AddCheck<ApiAdvancedSearchHealthCheck>("ApiAdvancedSearch", tags: tagsAddressSearchAPI)
            .AddCheck<ApiNearestAddressesHealthCheck>("ApiNearestAddresses", tags: tagsAddressNearestAPI)
            // Add a default liveness check to ensure app is responsive
            .AddCheck("self", () => HealthCheckResult.Healthy(), tagsLive);

        if (messagingSettings.Enabled)
        {
            // Add Azure Service Bus checks
            builder
                .AddAzureServiceBusQueue(messagingSettings.ConnectionString, messagingSettings.QueueName, tags: tagsServiceBusQueue)
                .AddAzureServiceBusQueueMessageCountThreshold(messagingSettings.ConnectionString, messagingSettings.QueueName, tags: tagsServiceBusThreshold)
                .AddAzureServiceBusSubscription(messagingSettings.ConnectionString, messagingSettings.TopicName, messagingSettings.SubscriptionName, tags: tagsServiceBusSubscription)
                .AddAzureServiceBusTopic(messagingSettings.ConnectionString, messagingSettings.TopicName, tags: tagsServiceBusTopic);
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

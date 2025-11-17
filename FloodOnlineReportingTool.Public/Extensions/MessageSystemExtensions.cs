using FloodOnlineReportingTool.Database.DbContexts;
using FloodOnlineReportingTool.Public.Options;
using MassTransit;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.AspNetCore.Builder;
#pragma warning restore IDE0130 // Namespace does not match folder structure

internal static class MessageSystemExtensions
{
    /// <summary>
    /// Add the message system. The Public project only needs to publish messages, not consume them
    /// </summary>
    /// <remarks>Even if messaging is disabled we still need to add MassTransit, so the database services work with the MassTransit interfaces.</remarks>
    internal static IServiceCollection AddMessageSystem(this IServiceCollection services, MessagingOptions messagingSettings)
    {
        if (!messagingSettings.Enabled)
        {
            services.AddMassTransit(o =>
            {
                // In-Memory transport configuration
                o.UsingInMemory((context, cfg) =>
                {
                    cfg.ConfigureEndpoints(context);
                });
            });

            return services;
        }

        services.AddMassTransit(o =>
        {
            var assembly = typeof(Program).Assembly;

            o.SetKebabCaseEndpointNameFormatter();

            // Add the outbox pattern
            o.AddEntityFrameworkOutbox<PublicDbContext>(config =>
            {
                config.UsePostgres();
                config.UseBusOutbox();
            });

            o.UsingAzureServiceBus((context, config) =>
            {
                if (messagingSettings.ConnectionString.Contains("Endpoint=", StringComparison.OrdinalIgnoreCase))
                {
                    config.Host(messagingSettings.ConnectionString);
                }
                else
                {
                    // Using Azure Managed Identity
                    config.Host(new Uri(messagingSettings.ConnectionString));
                }
            });
        });

        return services;
    }
}

using FloodOnlineReportingTool.Contracts;
using FloodOnlineReportingTool.Contracts.Topics;
using FloodOnlineReportingTool.Database.DbContexts;
using MassTransit;

namespace Microsoft.AspNetCore.Builder;

internal static class MessageSystemExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        /// <summary>
        /// Add the message system. The Public project only needs to publish messages, not consume them
        /// </summary>
        /// <remarks>Even if messaging is disabled we still need to add MassTransit, so the database services work with the MassTransit interfaces.</remarks>
        internal IHostApplicationBuilder AddMessageSystem()
        {
            var connectionString = builder.Configuration.GetConnectionString("service-bus");
            var useMessaging = !string.IsNullOrWhiteSpace(connectionString);

            if (!useMessaging)
            {
                builder.Services.AddMassTransit(o =>
                {
                    // In-Memory transport configuration
                    o.UsingInMemory((context, cfg) =>
                    {
                        cfg.ConfigureEndpoints(context);
                    });
                });

                return builder;
            }

            builder.Services.AddMassTransit(o =>
            {
                // Add the outbox pattern
                o.AddEntityFrameworkOutbox<PublicDbContext>(config =>
                {
                    config.UsePostgres();
                    config.UseBusOutbox();
                });

                o.UsingAzureServiceBus((context, config) =>
                {
                    if (connectionString!.Contains("Endpoint=", StringComparison.OrdinalIgnoreCase))
                    {
                        config.Host(connectionString);
                    }
                    else
                    {
                        // Using Azure Managed Identity
                        config.Host(new Uri(connectionString));
                    }

                    // Override MassTransit's default '~' convention to use simple and clean topic names
                    config.Message<EligibilityCheckCreated>(m => m.SetEntityName(TopicNames.EligibilityCheckCreated));
                    config.Message<FloodReportCreated>(m => m.SetEntityName(TopicNames.FloodReportCreated));
                });
            });

            return builder;
        }
    }
}

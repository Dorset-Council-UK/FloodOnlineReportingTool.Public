using FloodOnlineReportingTool.DataAccess.DbContexts;
using FloodOnlineReportingTool.DataAccess.Exceptions;
using FloodOnlineReportingTool.Public.Settings;
using MassTransit;

namespace Microsoft.AspNetCore.Builder;

internal static class MessageSystemExtensions
{
    /// <summary>
    /// Add the message system.
    /// </summary>
    public static IServiceCollection AddMessageSystem(this IServiceCollection services, IConfigurationSection? rabbitMqSection)
    {
        var rabbitMqSettings = rabbitMqSection?.Get<RabbitMqSettings>();
        bool isRabbitMqEnabled = rabbitMqSettings?.Enabled == true;

        // Even if messaging is disabled we still need to add MassTransit, so the services work with the MassTransit interfaces.

        services.AddMassTransit(x =>
        {
            // Message transport and configure endpoints
            if (isRabbitMqEnabled)
            {
                if (rabbitMqSettings is null)
                {
                    throw new ConfigurationMissingException("Missing configuration setting: The RabbitMQ configuration settings are missing");
                }

                x.SetKebabCaseEndpointNameFormatter(); // Human readable endpoint names

                // Add the outbox pattern
                x.AddEntityFrameworkOutbox<FORTDbContext>(o =>
                {
                    //o.QueryDelay = TimeSpan.FromSeconds(10); // For development testing only
                    o.UsePostgres();
                    o.UseBusOutbox();
                });

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(rabbitMqSettings.Host, h =>
                    {
                        h.Username(rabbitMqSettings.Username);
                        h.Password(rabbitMqSettings.Password);
                    });
                    cfg.ConfigureEndpoints(context);
                });
            }
            else
            {
                // In-Memory transport configuration
                x.UsingInMemory((context, cfg) =>
                {
                    cfg.ConfigureEndpoints(context);
                });
            }
        });

        return services;
    }
}

using FloodOnlineReportingTool.Database.DbContexts;
using Microsoft.EntityFrameworkCore;
using Messaging;
using ServiceDefaults;
using Microsoft.Extensions.Azure;
using Azure.Messaging.ServiceBus;
using Messaging.Consumers;
using FloodOnlineReportingTool.Database.Repositories;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

// Services
builder.Services
    .AddScoped<ICommonRepository, CommonRepository>()
    .AddScoped<IFloodReportSourceRepository, FloodReportSourceRepository>();

// Consumers
builder.Services
    .AddSingleton<IInboxConsumer, FloodReportExtraInfoRequestConsumer>();

builder.Services.AddHostedService<OutboxWorker>();
builder.Services.AddHostedService<InboxWorker>();

// add database
string? databaseConnectionString = builder.Configuration.GetConnectionString(ConnectionStringNames.Public);
if (string.IsNullOrWhiteSpace(databaseConnectionString))
{
    throw new InvalidOperationException($"Missing configuration setting: The database connection string '{ConnectionStringNames.Public}' is missing.");
}
builder.Services.AddDbContextFactory<PublicDbContext>(options => options.UseNpgsql(databaseConnectionString, npgsqlOptions => npgsqlOptions.UseNetTopologySuite()));
builder.EnrichNpgsqlDbContext<PublicDbContext>();

// add boundaries database (referenced by ICommonRepository)
if (string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString(ConnectionStringNames.Boundaries)))
{
    throw new InvalidOperationException($"Missing configuration setting: The boundaries database connection string '{ConnectionStringNames.Boundaries}' is missing.");
}
builder.AddNpgsqlDbContext<BoundariesDbContext>(ConnectionStringNames.Boundaries,
    configureDbContextOptions: options => options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));
builder.EnrichNpgsqlDbContext<BoundariesDbContext>();

// add service bus
string? serviceBusConnectionString = builder.Configuration.GetConnectionString(ConnectionStringNames.ServiceBus);
if (string.IsNullOrWhiteSpace(serviceBusConnectionString))
{
    throw new InvalidOperationException($"Missing configuration setting: The service bus connection string '{ConnectionStringNames.ServiceBus}' is missing.");
}
builder.Services.AddAzureClients(clientBuilder =>
{
    clientBuilder
        .AddServiceBusClient(serviceBusConnectionString)
        .ConfigureOptions(options =>
        {
            options.TransportType = ServiceBusTransportType.AmqpWebSockets;
        });
});

await builder.Build().RunAsync();

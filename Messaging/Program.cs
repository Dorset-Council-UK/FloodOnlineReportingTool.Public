using Azure.Messaging.ServiceBus;
using FloodOnlineReportingTool.Database.DbContexts;
using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.Database.Services;
using Messaging;
using Messaging.Consumers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using ServiceDefaults;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

// Services
builder.Services
    .AddScoped<ICommonRepository, CommonRepository>()
    .AddScoped<IFloodReportSourceRepository, FloodReportSourceRepository>()
    .AddScoped<IUserContext, UserContext>()
    .AddScoped<ISubscribeRecordRepository, SubscribeRecordRepository>();

// Consumers
builder.Services
    .AddSingleton<IInboxConsumer, FloodReportExtraInfoRequestConsumer>()
    .AddSingleton<IInboxConsumer, FloodReportSourceVerifyContactConsumer>();

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

using FloodOnlineReportingTool.Database.DbContexts;
using Microsoft.EntityFrameworkCore;
using Outbox;
using ServiceDefaults;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddHostedService<Worker>();

// add database
string? connectionString = builder.Configuration.GetConnectionString(ConnectionStringNames.Public);
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new Exception($"Missing configuration setting: The database connection string '{ConnectionStringNames.Public}' is missing.");
}
builder.Services.AddDbContextPool<PublicDbContext>(options => options.UseNpgsql(connectionString));
builder.EnrichNpgsqlDbContext<PublicDbContext>();

// add service bus
if (string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString(ConnectionStringNames.ServiceBus)))
{
    throw new Exception($"Missing configuration setting: The service bus connection string '{ConnectionStringNames.ServiceBus}' is missing.");
}
builder.AddAzureServiceBusClient(ConnectionStringNames.ServiceBus);

await builder.Build().RunAsync();

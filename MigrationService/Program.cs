using FloodOnlineReportingTool.Database.DbContexts;
using MigrationService;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddHostedService<Worker>();

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddSource(Worker.ActivitySourceName));

builder.AddNpgsqlDbContext<PublicDbContext>("FloodReportingPublic",
    configureDbContextOptions: options =>
        options.UseNpgsql(npgsql =>
            npgsql.MigrationsHistoryTable("__EFMigrationsHistory", SchemaNames.FortPublic)));

await builder.Build().RunAsync();

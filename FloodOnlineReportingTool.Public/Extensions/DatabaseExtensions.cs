using FloodOnlineReportingTool.Database.DbContexts;
using FloodOnlineReportingTool.Database.Exceptions;
using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.Database.Services;
using Microsoft.EntityFrameworkCore;
using ServiceDefaults;

namespace Microsoft.AspNetCore.Builder;

internal static class DatabaseExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        internal IHostApplicationBuilder AddFloodReportingDatabase()
        {
            var connectionString = builder.Configuration.GetConnectionString(ConnectionStringNames.Public);
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ConfigurationMissingException($"Missing configuration setting: The public connection string '{ConnectionStringNames.Public}' is missing.");
            }

            builder.Services.AddDbContextFactory<PublicDbContext>(options =>
            {
                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", SchemaNames.FortPublic);
                });
            });
            builder.EnrichNpgsqlDbContext<PublicDbContext>();

            return builder;
        }
        
        internal IHostApplicationBuilder AddBoundariesDatabase()
        {
            builder.AddNpgsqlDbContext<BoundariesDbContext>(ConnectionStringNames.Boundaries,
                configureDbContextOptions: options =>
                    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

            return builder;
        }

        internal IHostApplicationBuilder AddFloodReportingDatabaseServices()
        {
            builder.Services
                .AddScoped<ICommonRepository, CommonRepository>()
                .AddScoped<IContactRecordRepository, ContactRecordRepository>()
                .AddScoped<IEligibilityCheckRepository, EligibilityCheckRepository>()
                .AddScoped<IFloodReportRepository, FloodReportRepository>()
                .AddScoped<IInvestigationRepository, InvestigationRepository>()
                .AddScoped<IMediaItemRepository, MediaItemRepository>()
                .AddScoped<IOutboxMessageService, OutboxMessageService>()
                .AddScoped<ISearchRepository, SearchRepository>()
                .AddScoped<ISubscribeRecordRepository, SubscribeRecordRepository>();

            return builder;
        }
    }
}

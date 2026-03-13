using FloodOnlineReportingTool.Database.DbContexts;
using FloodOnlineReportingTool.Database.Exceptions;
using FloodOnlineReportingTool.Database.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.AspNetCore.Builder;

internal static class DatabaseExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        internal IHostApplicationBuilder AddFloodReportingDatabase()
        {
            var connectionString = builder.Configuration.GetConnectionString("FloodReportingPublic");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ConfigurationMissingException("Missing configuration setting: The public connection string 'FloodReportingPublic' is missing.");
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
            builder.AddNpgsqlDbContext<BoundariesDbContext>("Boundaries",
                configureDbContextOptions: options =>
                    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

            return builder;
        }

        internal IHostApplicationBuilder AddFloodReportingUsersDatabase()
        {
            var connectionString = builder.Configuration.GetConnectionString("FloodReportingUsers");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ConfigurationMissingException("Missing configuration setting: The users connection string 'FloodReportingUsers' is missing.");
            }

            builder.Services.AddDbContextFactory<UserDbContext>(options =>
            {
                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", SchemaNames.FortUsers);
                });
            });
            builder.EnrichNpgsqlDbContext<UserDbContext>();

            return builder;
        }

        internal IHostApplicationBuilder AddFloodReportingDatabaseRepositories()
        {
            builder.Services
                .AddScoped<ICommonRepository, CommonRepository>()
                .AddScoped<IContactRecordRepository, ContactRecordRepository>()
                .AddScoped<IEligibilityCheckRepository, EligibilityCheckRepository>()
                .AddScoped<IFloodReportRepository, FloodReportRepository>()
                .AddScoped<IInvestigationRepository, InvestigationRepository>()
                .AddScoped<ISearchRepository, SearchRepository>();

            return builder;
        }
    }
}

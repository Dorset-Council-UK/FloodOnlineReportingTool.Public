using FloodOnlineReportingTool.Database.DbContexts;
using FloodOnlineReportingTool.Database.Exceptions;
using FloodOnlineReportingTool.Database.Repositories;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Microsoft.AspNetCore.Builder;

internal static class DatabaseExtensions
{
    private static (string ConnectionString, string SearchPath) GetConnectionStringAndSearchPath(string? connectionString)
    {
        var connectionStringBuilder = new NpgsqlConnectionStringBuilder(connectionString);

        if (string.IsNullOrWhiteSpace(connectionStringBuilder.ConnectionString))
        {
            throw new ConfigurationMissingException("Missing configuration setting: The connection string is missing.");
        }

        if (string.IsNullOrWhiteSpace(connectionStringBuilder.SearchPath))
        {
            throw new ConfigurationMissingException("Missing configuration setting: The connection string needs a schema set in the Search Path parameter.");
        }

        return (connectionStringBuilder.ConnectionString, connectionStringBuilder.SearchPath);
    }

    extension(IHostApplicationBuilder builder)
    {
        internal IHostApplicationBuilder AddFloodReportingDatabase()
        {
            // It looks like the standard way to add the DbContext with Aspire is:
            // builder.AddNpgsqlDbContext<MyDbContext>("postgresdb");
            // But when more setup is needed, like with Blazor you can use:
            // var connectionString = builder.Configuration.GetConnectionString("postgresdb");
            // builder.Services.AddDbContextFactory<MyDbContext>(dbContextOptionsBuilder => dbContextOptionsBuilder.UseNpgsql(connectionString));
            // builder.EnrichNpgsqlDbContext<MyDbContext>();

            var (connectionString, searchPath) = GetConnectionStringAndSearchPath(builder.Configuration.GetConnectionString("FloodReportingPublic"));
            builder.Services.AddDbContextFactory<PublicDbContext>(options =>
            {
                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", searchPath);
                });
            });
            builder.EnrichNpgsqlDbContext<PublicDbContext>();

            return builder;
        }
        
        internal IHostApplicationBuilder AddBoundariesDatabase()
        {
            var (connectionString, searchPath) = GetConnectionStringAndSearchPath(builder.Configuration.GetConnectionString("boundariesdb"));

            // Add the database context (Because I am using OnConfiguring the DbContextPool cannot be used)
            builder.AddNpgsqlDbContext<BoundariesDbContext>("boundariesdb");

            return builder;
        }

        internal IHostApplicationBuilder AddFloodReportingUsersDatabase()
        {
            var (connectionString, searchPath) = GetConnectionStringAndSearchPath(builder.Configuration.GetConnectionString("FloodReportingUsers"));
            builder.Services.AddDbContextFactory<UserDbContext>(options =>
            {
                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", searchPath);
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

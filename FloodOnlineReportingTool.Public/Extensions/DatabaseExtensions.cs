using FloodOnlineReportingTool.DataAccess.DbContexts;
using FloodOnlineReportingTool.DataAccess.Exceptions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Microsoft.AspNetCore.Builder;

internal static class DatabaseExtensions
{
    public static IServiceCollection AddFloodReportingDatabase(this IServiceCollection services, string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ConfigurationMissingException("Missing configuration setting: The connection string for the flood reporting database is is missing.");
        }

        var builder = new NpgsqlConnectionStringBuilder(connectionString);
#if DEBUG
        builder.IncludeErrorDetail = true;
#endif

        // Double check that a schema has been set
        if (string.IsNullOrWhiteSpace(builder.SearchPath))
        {
            throw new ConfigurationMissingException("Missing configuration setting: The connection string for the flood reporting database needs a schema set in the Search Path parameter.");
        }

        // Add the database context
        services.AddDbContextPool<FORTDbContext>(
            options =>
            {
                options.UseNpgsql(builder.ToString(), o =>
                {
                    //o.EnableRetryOnFailure(5);
                    o.MigrationsHistoryTable("__EFMigrationsHistory", builder.SearchPath);
                });
#if DEBUG
                options.EnableSensitiveDataLogging(true);
#endif
            });

        return services;
    }

    public static IServiceCollection AddBoundariesDatabase(this IServiceCollection services, string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ConfigurationMissingException("Missing configuration setting: The connection string for the boundaries database is is missing.");
        }

        var builder = new NpgsqlConnectionStringBuilder(connectionString);
#if DEBUG
        builder.IncludeErrorDetail = true;
#endif

        // Double check that a schema has been set
        if (string.IsNullOrWhiteSpace(builder.SearchPath))
        {
            throw new ConfigurationMissingException("Missing configuration setting: The connection string for the boundaries database needs a schema set in the Search Path parameter.");
        }

        // Add the database context (Because I am using OnConfiguring the DbContextPool cannot be used)
        services.AddDbContext<BoundariesDbContext>(
            options =>
            {
                options.UseNpgsql(builder.ToString());
            });

        return services;
    }

    public static IServiceCollection AddFloodReportingUsersDatabase(this IServiceCollection services, string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ConfigurationMissingException("Missing configuration setting: The connection string for the identity database is is missing.");
        }

        var builder = new NpgsqlConnectionStringBuilder(connectionString);
#if DEBUG
        builder.IncludeErrorDetail = true;
#endif

        // Double check that a schema has been set
        if (string.IsNullOrWhiteSpace(builder.SearchPath))
        {
            throw new ConfigurationMissingException("Missing configuration setting: The connection string for the identity database needs a schema set in the Search Path parameter.");
        }

        // Add the database context
        services.AddDbContextPool<UserDbContext>(
            options =>
            {
                options.UseNpgsql(builder.ToString(), o =>
                {
                    o.MigrationsHistoryTable("__EFMigrationsHistory", builder.SearchPath);
                });
#if DEBUG
                options.EnableSensitiveDataLogging(true);
#endif
            });

        return services;
    }
}

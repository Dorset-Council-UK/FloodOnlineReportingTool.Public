using FloodOnlineReportingTool.Database.Repositories;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

internal static class RepositoryExtensions
{
    internal static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services
            .AddScoped<ICommonRepository, CommonRepository>()
            .AddScoped<IContactRecordRepository, ContactRecordRepository>()
            .AddScoped<IEligibilityCheckRepository, EligibilityCheckRepository>()
            .AddScoped<IFloodReportRepository, FloodReportRepository>()
            .AddScoped<IInvestigationRepository, InvestigationRepository>()
            .AddScoped<ISearchRepository, SearchRepository>();

        return services;
    }
}

using FloodOnlineReportingTool.DataAccess.Repositories;

namespace Microsoft.Extensions.DependencyInjection;

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

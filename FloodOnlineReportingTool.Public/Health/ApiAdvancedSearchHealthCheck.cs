using FloodOnlineReportingTool.Database.Repositories;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FloodOnlineReportingTool.Public.Health;

public class ApiAdvancedSearchHealthCheck(ISearchRepository searchRepository, IHttpContextAccessor httpContextAccessor) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct = default)
    {
        try
        {
            await searchRepository
                .IsAddressSearchAvailable(GetReferrer(), ct)
                .ConfigureAwait(false);

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(exception: ex);
        }
    }

    private Uri? GetReferrer()
    {
        var context = httpContextAccessor.HttpContext;
        if (context is null)
        {
            return null;
        }

        var referer = context.Request.GetTypedHeaders()?.Referer;
        return referer ?? context.Request.GetUri();
    }
}

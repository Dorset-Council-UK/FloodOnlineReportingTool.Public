using FloodOnlineReportingTool.Database.Models.API;
using FloodOnlineReportingTool.Database.Repositories;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FloodOnlineReportingTool.Public.Health;

public class ApiNearestAddressesHealthCheck(ISearchRepository searchRepository, IHttpContextAccessor httpContextAccessor) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct = default)
    {
        try
        {
            await searchRepository.IsNearestAddressAvailable(GetBaseUri(), SearchAreaOptions.dorset, ct);
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Nearest addresses health check failed", ex);
        }
    }

    private Uri? GetBaseUri()
    {
        var request = httpContextAccessor.HttpContext?.Request;
        if (request is null)
        {
            return null;
        }

        return new Uri($"{request.Scheme}://{request.Host}");
    }
}

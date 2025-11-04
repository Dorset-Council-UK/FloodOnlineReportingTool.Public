using FloodOnlineReportingTool.Public.Models;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace FloodOnlineReportingTool.Public.Services;

public class SessionStateService
{
    private readonly ProtectedSessionStorage _sessionStorage;
    private ILogger<SessionStateService> _logger;

    public SessionStateService(ProtectedSessionStorage sessionStorage, ILogger<SessionStateService> logger)
    {
        _sessionStorage = sessionStorage;
        _logger = logger;
    }

    public async Task<Guid> GetFloodReportId()
    {
        try
        {
            var storedId = await _sessionStorage.GetAsync<Guid>(SessionConstants.FloodReportId).ConfigureAwait(false);
            return storedId.Success ? storedId.Value : Guid.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return Guid.Empty;
        }
    }

    public async Task SaveFloodReportId(Guid floodReportId)
    {
        await _sessionStorage.SetAsync(SessionConstants.FloodReportId, floodReportId);
    }
}

using FloodOnlineReportingTool.Public.Models;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace FloodOnlineReportingTool.Public.Services;

public class SessionStateService
{
    private readonly ProtectedSessionStorage _sessionStorage;
    private readonly ILogger<SessionStateService> _logger;

    public SessionStateService(ProtectedSessionStorage sessionStorage, ILogger<SessionStateService> logger)
    {
        _sessionStorage = sessionStorage;
        _logger = logger;
    }

    public async Task<Guid> GetFloodReportSourceId()
    {
        try
        {
            var storedId = await _sessionStorage.GetAsync<Guid>(SessionConstants.FloodReportSourceId);
            return storedId.Success ? storedId.Value : Guid.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error getting flood report source ID from protected storeage: {ErrorMessage}", ex.Message);
            return Guid.Empty;
        }
    }

    public async Task SaveFloodReporSourcetId(Guid floodReportSourceId)
    {
        await _sessionStorage.SetAsync(SessionConstants.FloodReportSourceId, floodReportSourceId);
    }

    public async Task<Guid> GetVerificationId()
    {
        try
        {
            var storedId = await _sessionStorage.GetAsync<Guid>(SessionConstants.VerificationId);
            return storedId.Success ? storedId.Value : Guid.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error getting verification ID from protected storeage: {ErrorMessage}", ex.Message);
            return Guid.Empty;
        }
    }

    public async Task SaveVerificationId(Guid verificationId)
    {
        await _sessionStorage.SetAsync(SessionConstants.VerificationId, verificationId);
    }

}

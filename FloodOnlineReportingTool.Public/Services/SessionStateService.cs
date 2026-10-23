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
            _logger.LogError(ex, "Error getting flood report source ID from protected storage: {ErrorMessage}", ex.Message);
            return Guid.Empty;
        }
    }

    public async Task SaveFloodReporSourceId(Guid floodReportSourceId)
    {
        await _sessionStorage.SetAsync(SessionConstants.FloodReportSourceId, floodReportSourceId);
    }

    /// <summary>
    /// Gets whether the confirmation page's anonymous access has already been consumed.
    /// </summary>
    public async Task<bool> GetConfirmationAccessConsumed()
    {
        try
        {
            var storedValue = await _sessionStorage.GetAsync<bool>(SessionConstants.ConfirmationAccessConsumed);
            return storedValue.Success && storedValue.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting confirmation access state from protected storage: {ErrorMessage}", ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Stores whether the confirmation page's anonymous access has been consumed.
    /// </summary>
    public async Task SaveConfirmationAccessConsumed(bool consumed)
    {
        await _sessionStorage.SetAsync(SessionConstants.ConfirmationAccessConsumed, consumed);
    }

    /// <summary>
    /// Gets whether the confirmation page is waiting for a login handoff to complete.
    /// </summary>
    public async Task<bool> GetConfirmationLoginPending()
    {
        try
        {
            var storedValue = await _sessionStorage.GetAsync<bool>(SessionConstants.ConfirmationLoginPending);
            return storedValue.Success && storedValue.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting confirmation login state from protected storage: {ErrorMessage}", ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Stores whether the confirmation page is waiting for a login handoff to complete.
    /// </summary>
    public async Task SaveConfirmationLoginPending(bool pending)
    {
        await _sessionStorage.SetAsync(SessionConstants.ConfirmationLoginPending, pending);
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
            _logger.LogError(ex, "Error getting verification ID from protected storage: {ErrorMessage}", ex.Message);
            return Guid.Empty;
        }
    }

    public async Task SaveVerificationId(Guid verificationId)
    {
        await _sessionStorage.SetAsync(SessionConstants.VerificationId, verificationId);
    }

}

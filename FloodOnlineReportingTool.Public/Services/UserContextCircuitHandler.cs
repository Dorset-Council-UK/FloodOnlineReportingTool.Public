using FloodOnlineReportingTool.Database.Services;
using Microsoft.AspNetCore.Components.Server.Circuits;

namespace FloodOnlineReportingTool.Public.Services;

// Investigate why this works? 
// https://learn.microsoft.com/en-us/aspnet/core/blazor/fundamentals/signalr?view=aspnetcore-10.0
// https://learn.microsoft.com/en-us/aspnet/core/blazor/security/
public class UserContextCircuitHandler : CircuitHandler
{
    private readonly IUserContext _userContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContextCircuitHandler(
        IUserContext userContext,
        IHttpContextAccessor httpContextAccessor)
    {
        _userContext = userContext;
        _httpContextAccessor = httpContextAccessor;
    }

    public override Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        // Capture user during initial HTTP connection (before SignalR takes over)
        var user = _httpContextAccessor.HttpContext?.User;
        _userContext.SetUser(user);
        
        return base.OnConnectionUpAsync(circuit, cancellationToken);
    }
}
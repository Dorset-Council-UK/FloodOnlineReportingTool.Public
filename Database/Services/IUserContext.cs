using System.Security.Claims;

namespace FloodOnlineReportingTool.Database.Services;

public interface IUserContext
{
    bool CanViewPersonalData { get; }
    ClaimsPrincipal? User { get; }
    void SetUser(ClaimsPrincipal? user); 
}
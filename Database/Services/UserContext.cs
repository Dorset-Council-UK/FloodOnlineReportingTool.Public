using System.Security.Claims;

namespace FloodOnlineReportingTool.Database.Services;

public class UserContext : IUserContext
{
    public bool CanViewPersonalData { get; private set; }
    public ClaimsPrincipal? User { get; private set; }

    public void SetUser(ClaimsPrincipal? user)
    {
        User = user;
        CanViewPersonalData = user?.IsInRole("Public.PersonalData") == true;
    }
}
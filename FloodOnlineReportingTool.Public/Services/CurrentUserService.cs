using System.Security.Claims;

namespace FloodOnlineReportingTool.Public.Services;

internal sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public string UserId => User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

    public string Name => User?.FindFirstValue("name") ?? "";

    public string Email
    {
        get
        {
            var email = User?.FindFirstValue(ClaimTypes.Email);
            if (!string.IsNullOrWhiteSpace(email))
            {
                return email;
            }

            var preferredUsername = User?.FindFirstValue("preferred_username");
            return preferredUsername ?? "";
        }
    }
}

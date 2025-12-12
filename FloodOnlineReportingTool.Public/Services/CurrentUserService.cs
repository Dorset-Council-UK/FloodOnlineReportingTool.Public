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
            if (!string.IsNullOrWhiteSpace(email) && !email.EndsWith(".onmicrosoft.com", StringComparison.OrdinalIgnoreCase))
            {
                return email;
            }

            // Try "emails" claim (Azure B2C/External Identity often uses this)
            var emails = User?.FindFirstValue("emails");
            if (!string.IsNullOrWhiteSpace(emails) && !emails.EndsWith(".onmicrosoft.com", StringComparison.OrdinalIgnoreCase))
            {
                return emails;
            }

            // Last resort: preferred_username (but filter out .onmicrosoft.com)
            var preferredUsername = User?.FindFirstValue("preferred_username");
            if (!string.IsNullOrWhiteSpace(preferredUsername) && !preferredUsername.EndsWith(".onmicrosoft.com", StringComparison.OrdinalIgnoreCase))
            {
                return preferredUsername;
            }

            // Fallback to empty string if all else fails
            return "";
        }
    }
}

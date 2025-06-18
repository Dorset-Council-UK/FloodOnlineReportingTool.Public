using Microsoft.AspNetCore.Authentication.Cookies;

namespace FloodOnlineReportingTool.Public.Authentication;

internal static class AuthenticationSchemeDefaults
{
    internal const string CookieScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    internal const string ReturnUrlParameter = "redirect";
    internal const string FloodReportUserRole = "FloodReportUser";
    internal const string FloodReportAdminRole = "Admin";
}

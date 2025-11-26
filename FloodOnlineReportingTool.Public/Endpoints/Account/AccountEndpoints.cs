using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Identity.Web;
using System.IO;

namespace FloodOnlineReportingTool.Public.Endpoints.Account;

internal static class AccountEndpoints
{
    private static bool IsLocalPath(string? path)
    {

        if (string.IsNullOrWhiteSpace(path))
            return false;

        // Reject if it's an absolute URI (http://, https://, etc.)
        if (Uri.TryCreate(path, UriKind.Absolute, out _))
            return false;

        // Reject protocol-relative URLs (//example.com)
        if (path.StartsWith("//", StringComparison.OrdinalIgnoreCase))
            return false;

        // Accept relative paths like "contacts", "report-flooding/contacts", or "/report-flooding"
        return true;
  
    }

    internal static Results<ChallengeHttpResult, UnauthorizedHttpResult, ForbidHttpResult> SignIn(
        string? redirectUri,
        string? loginHint,
        string? domainHint
        
    )
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = IsLocalPath(redirectUri) ? redirectUri : "/report-flooding",
            Parameters =
            {
                { Constants.LoginHint, loginHint },
                { Constants.DomainHint, domainHint },
            },
        };

        return TypedResults.Challenge(properties, [CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme]);
    }

    internal static Results<SignOutHttpResult, UnauthorizedHttpResult> SignOut()
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = "/report-flooding/Account/signedout",
        };

        return TypedResults.SignOut(properties, [CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme]);
    }

    internal static Results<ChallengeHttpResult, UnauthorizedHttpResult, ForbidHttpResult> IdentityChallenge(
        string? redirectUri,
        string? scope,
        string? loginHint,
        string? domainHint,
        string? claims,
        string? policy,
        string? scheme
    )
    {
        scheme ??= OpenIdConnectDefaults.AuthenticationScheme;

        Dictionary<string, string?> items = new(StringComparer.Ordinal)
        {
            { Constants.Claims, claims },
            { Constants.Policy, policy },
        };
        Dictionary<string, object?> parameters = new(StringComparer.Ordinal)
        {
            { Constants.LoginHint, loginHint },
            { Constants.DomainHint, domainHint },
        };

        var oAuthChallengeProperties = new OAuthChallengeProperties(items, parameters);
        if (scope != null)
        {
            oAuthChallengeProperties.Scope = scope.Split(" ");
        }
        oAuthChallengeProperties.RedirectUri = redirectUri;

        return TypedResults.Challenge(oAuthChallengeProperties, [scheme]);
    }
}

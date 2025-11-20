using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Identity.Web;

namespace FloodOnlineReportingTool.Public.Endpoints.Account;

internal static class AccountEndpoints
{
    private static bool IsLocalUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        // Only allow relative URLs that start with a single '/'
        if (url[0] == '/')
        {
            if (url.Length == 1)
            {
                return true;
            }

            // "/foo" is local, but "//foo" and "/\foo" are not
            if (url[1] != '/' && url[1] != '\\')
            {
                return true;
            }
        } else if(url[0] == '~' && url.StartsWith("http", StringComparison.OrdinalIgnoreCase) == true){
            // Disallow all other forms, including "~" style and absolute URLs
            return false;
        }

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
            RedirectUri = IsLocalUrl(redirectUri) ? redirectUri : "/report-flooding",
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

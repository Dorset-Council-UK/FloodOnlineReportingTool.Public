using FloodOnlineReportingTool.Database.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;

namespace FloodOnlineReportingTool.Public.Endpoints.Account;

internal static class AccountEndpoints
{
    private static string? NormaliseRedirectUrl(string? redirectUri, string pathBase)
    {
        if (string.IsNullOrWhiteSpace(redirectUri))
        {
            return null;
        }

        if (RedirectHttpResult.IsLocalUrl(redirectUri))
        {
            return redirectUri;
        }

        // Convert relative to absolute by prepending /
        var pathWithoutLeadingSlash = redirectUri.TrimStart('/');
        var absolutePath = $"/{pathBase}/{pathWithoutLeadingSlash}";
        return RedirectHttpResult.IsLocalUrl(absolutePath) ? absolutePath : null;
    }

    internal static Results<ChallengeHttpResult, UnauthorizedHttpResult, ForbidHttpResult> SignIn(
        IOptions<GISOptions> options,
        string? redirectUri,
        string? loginHint,
        string? domainHint
    ) {
        var normalisedRedirectUri = NormaliseRedirectUrl(redirectUri, options.Value.PathBase);

        var properties = new AuthenticationProperties
        {
            RedirectUri = normalisedRedirectUri ?? $"/{options.Value.PathBase}",
            Parameters =
            {
                { Constants.LoginHint, loginHint },
                { Constants.DomainHint, domainHint },
            },
        };

        return TypedResults.Challenge(properties, [CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme]);
    }

    internal static Results<SignOutHttpResult, UnauthorizedHttpResult> SignOut(IOptions<GISOptions> options)
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = $"/{options.Value.PathBase}/Account/signedout",
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
    ) {
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

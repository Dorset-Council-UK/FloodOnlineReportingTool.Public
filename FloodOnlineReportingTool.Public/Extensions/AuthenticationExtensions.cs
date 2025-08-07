using FloodOnlineReportingTool.Public.Authentication;
using FloodOnlineReportingTool.Public.Models.Order;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Identity.Web;

namespace Microsoft.AspNetCore.Builder;

internal static class AuthenticationExtensions
{
    /// <summary>
    ///     <para>Configures authentication, authorization, and policies.</para>
    ///     <para>Sets up Identity / Identity Platform / Entra / Bearer authentication for role based access, using C# policies.</para>
    /// </summary>
    internal static IServiceCollection AddFloodReportingAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var authenticationBuilder = BuildIdentityAuthentication(services);

        BuildIdentityPlatform(services, authenticationBuilder, configuration);

        // Add Blazor cascading authentication state
        services.AddCascadingAuthenticationState();

        BuildAuthentication(services);

        return services;
    }

    /// <summary>
    /// Build the Identity authentication scheme and settings.
    /// </summary>
    private static AuthenticationBuilder BuildIdentityAuthentication(IServiceCollection services)
    {
        return services
            .AddAuthentication(IdentityConstants.ApplicationScheme)
            .AddCookie(IdentityConstants.ApplicationScheme, options =>
            {
                options.Cookie.IsEssential = true;
                options.Cookie.Name = "FloodReportCookie";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.ReturnUrlParameter = "returnUrl";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                options.LoginPath = "/" + AccountPages.SignIn.Url;
                options.LogoutPath = "/" + AccountPages.SignOut.Url;
                options.AccessDeniedPath = "/" + GeneralPages.AccessDenied;
                options.SlidingExpiration = true;
            });
    }

    /// <summary>
    /// Build the Identity Platform authentication scheme and settings. Used to protect the Identity Platform API endpoints.
    /// </summary>
    private static void BuildIdentityPlatform(IServiceCollection services, AuthenticationBuilder authenticationBuilder, IConfiguration configuration)
    {
        if (!configuration.GetSection(Constants.AzureAd).Exists())
        {
            return;
        }

        // Resilient HTTP client for authentication purposes. For example: retrying when .well-known fails to read.
        const string clientName = "OAuthResilient";
        services
            .AddHttpClient(clientName)
            .AddStandardResilienceHandler();

        var httpClientFactory = services.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();
        var backingChannel = httpClientFactory.CreateClient(clientName);

        authenticationBuilder
            .AddMicrosoftIdentityWebApi(jwtBearerOptions =>
            {
                configuration.Bind(Constants.AzureAd, jwtBearerOptions);
                jwtBearerOptions.Backchannel = backingChannel;
            },
            microsoftIdentityOptions =>
            {
                configuration.Bind(Constants.AzureAd, microsoftIdentityOptions);
            });
    }

    private static void BuildAuthentication(IServiceCollection services)
    {
        services
            .AddAuthorizationBuilder()
            .AddPolicy(PolicyNames.Identity, policy =>
            {
                policy.AddAuthenticationSchemes(IdentityConstants.ApplicationScheme);
                policy.RequireAuthenticatedUser();
            })
            .AddPolicy(PolicyNames.Reader, policy => policy
                .RequireAuthenticatedUser()
                .RequireAssertion(context =>
                    context.User.IsInRole(RoleNames.Reader) ||
                    context.User.IsInRole(RoleNames.Admin)))
            .AddPolicy(PolicyNames.Admin, policy => policy
                .RequireAuthenticatedUser()
                .RequireRole(RoleNames.Admin));
    }
}

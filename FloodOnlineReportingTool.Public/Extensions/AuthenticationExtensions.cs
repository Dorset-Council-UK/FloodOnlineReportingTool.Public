using FloodOnlineReportingTool.Public.Authentication;
using FloodOnlineReportingTool.Public.Models.Order;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
        // Setup authentication
        BuildIdentityAuthentication(services, configuration);

        ConfigureResilientJwtBearerOptions(services);

        // Add Blazor cascading authentication state
        services.AddCascadingAuthenticationState();

        // Setup Authorization
        BuildAuthentication(services);

        return services;
    }

    /// <summary>
    /// Build the Identity authentication scheme and settings.
    /// </summary>
    private static void BuildIdentityAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        services
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
            })
            .AddMicrosoftIdentityWebApi(configuration);
    }

    /// <summary>
    /// Configure a resilient JWT Bearer Options authentication handler.
    /// </summary>
    /// <remarks>For example, retrying when .well-known fails to read.</remarks>
    private static void ConfigureResilientJwtBearerOptions(IServiceCollection services)
    {
        const string clientName = "OAuthResilient";

        services
            .AddHttpClient(clientName)
            .AddStandardResilienceHandler();

        services
            .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IHttpClientFactory>((options, httpClientFactory) =>
            {
                options.Backchannel = httpClientFactory.CreateClient(clientName);
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

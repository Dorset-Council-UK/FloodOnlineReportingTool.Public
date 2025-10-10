using FloodOnlineReportingTool.Public.Authentication;
using FloodOnlineReportingTool.Public.Models.Order;
using FloodOnlineReportingTool.Public.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Identity;
using Microsoft.Identity.Web;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.AspNetCore.Builder;
#pragma warning restore IDE0130 // Namespace does not match folder structure

internal static class AuthenticationExtensions
{
    /// <summary>
    /// Configures authentication, authorization and policies.
    /// </summary>
    internal static IServiceCollection AddFloodReportingAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration
            .GetSection("FORT")
            .GetSection(KeyVaultAzureAdOptions.SectionName);

        // Setup Authentication
        services
            .AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApp(section);

        ConfigureResilientOpenIdConnect(services);

        // Add Blazor cascading authentication state
        services.AddCascadingAuthenticationState();

        // Setup Authorization
        services
            .AddAuthorizationBuilder()
            .AddPolicy(PolicyNames.Admin, options =>
            {
                options.RequireAuthenticatedUser();
            });

        return services;
    }

    /// <summary>
    /// Configure a resilient OpenID Connect authentication handler.
    /// </summary>
    /// <remarks>For example, retrying when .well-known fails to read.</remarks>
    private static void ConfigureResilientOpenIdConnect(IServiceCollection services)
    {
        const string clientName = "OAuthResilient";

        services
            .AddHttpClient(clientName)
            .AddStandardResilienceHandler();

        services
            .AddOptions<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme)
            .Configure<IHttpClientFactory>((options, httpClientFactory) =>
            {
                options.Backchannel = httpClientFactory.CreateClient(clientName);
            });
    }
}

//internal static class AuthenticationExtensions
//{
//    /// <summary>
//    ///     <para>Configures authentication, authorization, and policies.</para>
//    ///     <para>Sets up Identity / Identity Platform / Entra / Bearer authentication for role based access, using C# policies.</para>
//    /// </summary>
//    internal static IServiceCollection AddFloodReportingAuthentication(this IServiceCollection services, IConfiguration configuration)
//    {
//        // Setup authentication
//        BuildIdentityAuthentication(services, configuration);

//        ConfigureResilientJwtBearerOptions(services);

//        // Add Blazor cascading authentication state
//        services.AddCascadingAuthenticationState();

//        // Setup Authorization
//        BuildAuthorization(services);

//        return services;
//    }

//    /// <summary>
//    ///     Build the Identity authentication scheme and settings.
//    /// </summary>
//    private static void BuildIdentityAuthentication(IServiceCollection services, IConfiguration configuration)
//    {
//        services
//            .AddAuthentication(IdentityConstants.ApplicationScheme)
//            .AddCookie(IdentityConstants.ApplicationScheme, options =>
//            {
//                options.Cookie.IsEssential = true;
//                options.Cookie.Name = "FloodReportCookie";
//                options.Cookie.HttpOnly = true;
//                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
//                options.Cookie.SameSite = SameSiteMode.Strict;
//                options.ReturnUrlParameter = "returnUrl";
//                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
//                options.LoginPath = "/" + AccountPages.SignIn.Url;
//                options.LogoutPath = "/" + AccountPages.SignOut.Url;
//                options.AccessDeniedPath = "/" + GeneralPages.AccessDenied;
//                options.SlidingExpiration = true;
//            })
//            .AddMicrosoftIdentityWebApi(configuration);
//    }

//    /// <summary>
//    /// Configure a resilient JWT Bearer Options authentication handler.
//    /// </summary>
//    /// <remarks>For example, retrying when .well-known fails to read.</remarks>
//    private static void ConfigureResilientJwtBearerOptions(IServiceCollection services)
//    {
//        const string clientName = "OAuthResilient";

//        services
//            .AddHttpClient(clientName)
//            .AddStandardResilienceHandler();

//        services
//            .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
//            .Configure<IHttpClientFactory>((options, httpClientFactory) =>
//            {
//                options.Backchannel = httpClientFactory.CreateClient(clientName);
//            });
//    }

//    private static void BuildAuthorization(IServiceCollection services)
//    {
//        services
//            .AddAuthorizationBuilder()
//            .AddPolicy(PolicyNames.Identity, policy =>
//            {
//                policy.AddAuthenticationSchemes(IdentityConstants.ApplicationScheme);
//                policy.RequireAuthenticatedUser();
//            })
//            .AddPolicy(PolicyNames.Reader, policy => policy
//                .RequireAuthenticatedUser()
//                .RequireAssertion(context =>
//                    context.User.IsInRole(RoleNames.Reader) ||
//                    context.User.IsInRole(RoleNames.Admin)))
//            .AddPolicy(PolicyNames.Admin, policy => policy
//                .RequireAuthenticatedUser()
//                .RequireRole(RoleNames.Admin));
//    }
//}

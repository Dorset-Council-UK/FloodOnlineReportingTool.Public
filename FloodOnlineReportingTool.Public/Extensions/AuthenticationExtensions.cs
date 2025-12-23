using FloodOnlineReportingTool.Public.Authentication;
using FloodOnlineReportingTool.Public.Endpoints.Account;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.AspNetCore.Builder;
#pragma warning restore IDE0130 // Namespace does not match folder structure

internal static class AuthenticationExtensions
{
    /// <summary>
    /// Configures authentication, authorization and policies.
    /// </summary>
    internal static TBuilder AddAuthentication<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        // Setup Authentication
        builder.Services
            .AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApp(options =>
            {
                builder.Configuration.Bind(Constants.AzureAd, options);

                // Ensure we're requesting and saving tokens
                options.SaveTokens = true;
                options.GetClaimsFromUserInfoEndpoint = true; // Try to get claims from UserInfo endpoint

                // Inject user flow as query parameter
                options.Events.OnRedirectToIdentityProvider = context =>
                {
                    context.ProtocolMessage.SetParameter("p", "Staging_Test_Flow");

                    return Task.CompletedTask;
                };

                // Log when UserInfo endpoint is called
                options.Events.OnUserInformationReceived = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    if (context.User != null && context.Principal != null)
                    {
                        // Try to find email in UserInfo response
                        if (context.User.RootElement.TryGetProperty("email", out var emailProperty))
                        {
                            var email = emailProperty.GetString();

                            // Manually add the email claim to the principal
                            var identity = context.Principal.Identity as System.Security.Claims.ClaimsIdentity;
                            if (identity != null && !string.IsNullOrWhiteSpace(email))
                            {
                                // Add as standard ClaimTypes.Email
                                identity.AddClaim(new System.Security.Claims.Claim(
                                    System.Security.Claims.ClaimTypes.Email,
                                    email));

                                // Also add as "emails" for compatibility with Azure B2C patterns
                                identity.AddClaim(new System.Security.Claims.Claim(
                                    "emails",
                                    email));

                                logger.LogInformation("Successfully added email claim to principal");
                            }
                        }
                        else
                        {
                            logger.LogWarning("No 'email' property found in UserInfo response!");
                        }
                    }

                    return Task.CompletedTask;
                };

            });

        // Configure all HttpClients to be resilient. For example: Identity Web + DownstreamApi
        builder.Services
            .AddHttpClient(Options.DefaultName)
            .AddStandardResilienceHandler();

        ConfigureResilientOpenIdConnect(builder.Services);

        // Add Blazor cascading authentication state
        builder.Services.AddCascadingAuthenticationState();

        // Setup Authorization
        builder.Services
            .AddAuthorizationBuilder()
            .AddPolicy(PolicyNames.Reader, policy => policy
                .RequireAuthenticatedUser()
                .RequireAssertion(context =>
                    context.User.IsInRole(RoleNames.Reader) ||
                    context.User.IsInRole(RoleNames.Admin)))
            .AddPolicy(PolicyNames.Admin, policy => policy
                .RequireAuthenticatedUser()
                .RequireRole(RoleNames.Admin))
            .SetFallbackPolicy(policy: null); // Anonymous access allowed

        return builder;
    }

    /// <summary>
    /// Configure a resilient OpenID Connect authentication handler.
    /// </summary>
    /// <remarks>For example, retrying when .well-known fails to read.</remarks>
    private static void ConfigureResilientOpenIdConnect(IServiceCollection services)
    {
        const string clientName = "OpenIdConnectResilient";

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

    internal static WebApplication MapAuthenticationEndpoints(this WebApplication app)
    {
        app.MapGet("signin", AccountEndpoints.SignIn)
            .WithTags("Account")
            .WithDisplayName("Sign in")
            .WithSummary("Signs the user into the application")
            .AllowAnonymous();

        app.MapGet("signout", AccountEndpoints.SignOut)
            .WithTags("Account")
            .WithDisplayName("Sign out")
            .WithSummary("Signs the user out of the application.")
            .AllowAnonymous();

        app.MapGet("MicrosoftIdentity/Account/Challenge", AccountEndpoints.IdentityChallenge)
            .WithTags("Account")
            .WithDisplayName("Microsoft Identity challenge")
            .WithSummary("Challenge generating a redirect to EntraID to sign in the user.")
            .AllowAnonymous();

        return app;
    }
}

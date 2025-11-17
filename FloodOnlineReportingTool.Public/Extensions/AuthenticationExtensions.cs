using FloodOnlineReportingTool.Public.Authentication;
using FloodOnlineReportingTool.Public.Options;
using MassTransit.Configuration;
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

        var azureAdSection = builder.Configuration
            .GetRequiredSection(AzureAdOptions.SectionName);

        // Setup Authentication
        builder.Services
            .AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApp(azureAdSection);

        ConfigureResilientOpenIdConnect(builder.Services);

        // Add Blazor cascading authentication state
        builder.Services.AddCascadingAuthenticationState();

        // Setup Authorization
        builder.Services
           .AddAuthorizationBuilder()
            .AddPolicy(PolicyNames.Admin, options =>
            {
                options.RequireAuthenticatedUser();
            });

        return builder;
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

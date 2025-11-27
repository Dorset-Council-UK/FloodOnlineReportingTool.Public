using Asp.Versioning;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;

namespace FloodOnlineReportingTool.Public.Models.OpenApi;

internal sealed class DocumentTransformer(ApiVersion version, MicrosoftIdentityOptions? identityOptions) : IOpenApiDocumentTransformer
{
    internal static string Title = "Flood Online Reporting Tool - Public API";
    internal static string Description = "Flood online reporting Public API and application";

    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        // dictionary or empty dictionary
        var scopes = identityOptions == null
            ? new Dictionary<string, string>(StringComparer.Ordinal)
            : new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { $"api://{identityOptions.ClientId}/access_as_user", "Access the API as the signed-in user" },
            };

        AddInformation(document);
        AddSecuritySchemes(document, scopes);
        AddSecurityRequirements(document, scopes);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Adds information to the OpenAPI document.
    /// </summary>
    private void AddInformation(OpenApiDocument document)
    {
        // Document information
        var versionString = version.ToString("VVVV", ApiVersionFormatProvider.CurrentCulture);
        document.Info = new(document.Info)
        {
            Version = versionString,
            Title = $"{Title} | v{versionString}",
            Description = Description,
            TermsOfService = new("https://github.com/Dorset-Council-UK/FloodOnlineReportingTool.Public"),
            Contact = new()
            {
                Name = "Contact",
                Url = new("https://github.com/Dorset-Council-UK/FloodOnlineReportingTool.Public"),
            },
            License = new()
            {
                Name = "MIT",
                Url = new("https://github.com/Dorset-Council-UK/FloodOnlineReportingTool.Public/blob/main/LICENSE.md"),
            },
        };
    }

    /// <summary>
    /// Adds security schemes to the OpenAPI document.
    /// </summary>
    private void AddSecuritySchemes(OpenApiDocument document, Dictionary<string, string> scopes)
    {
        var baseEndpoint = identityOptions != null
            ? new Uri(new Uri(identityOptions.Instance), $"{identityOptions.TenantId}/")
            : new Uri(new Uri("https://login.microsoftonline.com/"), "YOUR_TENANT_ID");
        var openIdConnectUrl = new Uri(baseEndpoint, "v2.0/.well-known/openid-configuration");
        var authorizationUrl = new Uri(baseEndpoint, "oauth2/v2.0/authorize");
        var tokenUrl = new Uri(baseEndpoint, "oauth2/v2.0/token");

        document.Components ??= new();
        document.Components.SecuritySchemes.Add(Constants.Bearer, new()
        {
            Name = "Authorization",
            Scheme = Constants.Bearer,
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.OAuth2,
            OpenIdConnectUrl = openIdConnectUrl,
            Flows = new()
            {
                AuthorizationCode = new()
                {
                    AuthorizationUrl = authorizationUrl,
                    TokenUrl = tokenUrl,
                    Scopes = scopes,
                },
            },
        });
    }

    /// <summary>
    /// Adds security requirements to the OpenAPI document.
    /// </summary>
    private static void AddSecurityRequirements(OpenApiDocument document, Dictionary<string, string> scopes)
    {
        document.SecurityRequirements.Add(new()
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new()
                    {
                        Id = Constants.Bearer,
                        Type = ReferenceType.SecurityScheme,
                    },
                },
                scopes.Select(scope => scope.Key).ToArray()
            },
        });
    }
}

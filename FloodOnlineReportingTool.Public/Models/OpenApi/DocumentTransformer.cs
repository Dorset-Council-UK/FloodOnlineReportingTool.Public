using Asp.Versioning;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace FloodOnlineReportingTool.Public.Models.OpenApi;

internal sealed class DocumentTransformer(ApiVersion version) : IOpenApiDocumentTransformer
{
    internal static string Title = "Flood Online Reporting Tool - Public API";
    internal static string Description = "Flood online reporting API and application";

    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        AddInformation(document);
        AddSecuritySchemes(document);
        AddSecurityRequirements(document);

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
                Url = new("https://github.com/Dorset-Council-UK/FloodOnlineReportingTool.Public/blob/main/LICENSE"),
            },
        };
    }

    /// <summary>
    /// Adds security schemes to the OpenAPI document.
    /// </summary>
    private static void AddSecuritySchemes(OpenApiDocument document)
    {
        // Note: OpenIdConnectUrl may not be needed unless Type is set to SecuritySchemeType.OpenIdConnect (OIDC) which might mean that the flow would need to be changed to Implicit.

        var baseEndpoint = new Uri(new Uri("https://login.microsoftonline.com/"), "TenantId/");
        var openIdConnectUrl = new Uri(baseEndpoint, "v2.0/.well-known/openid-configuration");
        var authorizationUrl = new Uri(baseEndpoint, "oauth2/v2.0/authorize");
        var tokenUrl = new Uri(baseEndpoint, "oauth2/v2.0/token");

        document.Components ??= new();
        document.Components.SecuritySchemes.Add("Bearer", new()
        {
            Name = "Authorization",
            Scheme = "Bearer",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.OAuth2,
            OpenIdConnectUrl = openIdConnectUrl,
            Flows = new()
            {
                AuthorizationCode = new()
                {
                    AuthorizationUrl = authorizationUrl,
                    TokenUrl = tokenUrl,
                },
            },
        });
    }

    /// <summary>
    /// Adds security requirements to the OpenAPI document.
    /// </summary>
    private static void AddSecurityRequirements(OpenApiDocument document)
    {
        document.SecurityRequirements.Add(new()
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new()
                    {
                        Id = "Bearer",
                        Type = ReferenceType.SecurityScheme,
                    },
                },
                Array.Empty<string>()
            },
        });
    }
}

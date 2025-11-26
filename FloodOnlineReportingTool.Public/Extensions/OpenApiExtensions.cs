using Asp.Versioning;
using FloodOnlineReportingTool.Public.Models.OpenApi;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Identity.Web;
using Scalar.AspNetCore;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.AspNetCore.Builder;
#pragma warning restore IDE0130 // Namespace does not match folder structure

internal static class OpenApiExtensions
{
    /// <summary>
    /// Add OpenApi using the versions
    /// </summary>
    /// <remarks>
    ///     <para>See: <see href="https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/customize-openapi?view=aspnetcore-9.0#use-document-transformers">Use document transformers</see></para>
    /// </remarks>
    internal static IServiceCollection AddFloodReportingOpenApi(this IServiceCollection services, MicrosoftIdentityOptions? identityOptions)
    {
        foreach (var versionInfo in Versions.All)
        {
            services.AddOpenApi(versionInfo.DocumentName, options =>
            {
                // Set the OpenAPI document info, security schemes, and security requirements
                options.AddDocumentTransformer(new DocumentTransformer(versionInfo.Version, identityOptions));

                // Add default responses for 401 not authorised
                options.AddOperationTransformer(new AuthOperationTransformer());

                options.ShouldInclude = description => ShouldIncludeVersion(description, versionInfo);
            });
        }

        return services;
    }

    /// <summary>
    /// Add API methods to the correct version documentation
    /// </summary>
    private static bool ShouldIncludeVersion(ApiDescription description, VersionInfo versionInfo)
    {
        if (description.GroupName != null && description.GroupName.Equals(versionInfo.DocumentName, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var versionMetadata = description.ActionDescriptor.EndpointMetadata
            .OfType<ApiVersionMetadata>()
            .FirstOrDefault();
        if (versionMetadata != null && versionMetadata.IsMappedTo(versionInfo.Version))
        {
            return true;
        }

        return true;
    }

    /// <summary>
    /// Add the Swagger UI <see href="https://localhost:7222/report-flooding/swagger" />
    /// </summary>
    internal static WebApplication UseSwagger(this WebApplication app, MicrosoftIdentityOptions? identityOptions)
    {
        app.UseSwaggerUI(options =>
        {
            options.DocumentTitle = DocumentTransformer.Title;
            options.EnableTryItOutByDefault();
            foreach (var versionInfo in Versions.All)
            {
                options.SwaggerEndpoint($"/openapi/{versionInfo.DocumentName}.json", $"{DocumentTransformer.Title} | {versionInfo.DocumentName}");
            }
            options.OAuthUsePkce();
            if (identityOptions != null)
            {
                options.OAuthClientId(identityOptions.ClientId);
                options.OAuthScopes($"api://{identityOptions.ClientId}/access_as_user");
            }
            else
            {
                options.OAuthClientId("YOUR_CLIENT_ID");
            }
        });

        return app;
    }

    /// <summary>
    /// Add the Scalar UI <see href="https://localhost:7222/report-flooding/scalar" />
    /// </summary>
    internal static WebApplication UseScalar(this WebApplication app, MicrosoftIdentityOptions? identityOptions)
    {
        var documentNames = Versions.All.Select(o => o.DocumentName);

        app.MapScalarApiReference(options =>
        {
            options
                .WithTitle(DocumentTransformer.Title)
                .SortOperationsByMethod()
                .SortTagsAlphabetically()
                .AddDocuments(documentNames)
                .AddPreferredSecuritySchemes(Constants.Bearer)
                .AddOAuth2Flows(Constants.Bearer, flows =>
                {
                    flows.WithAuthorizationCode(options =>
                    {
                        options.Pkce = Pkce.Sha256;
                        if (identityOptions != null)
                        {
                            options.ClientId = identityOptions.ClientId;
                            options.SelectedScopes = [$"api://{identityOptions.ClientId}/access_as_user"];
                        }
                        else
                        {
                            options.ClientId = "YOUR_CLIENT_ID";
                        }
                    });
                });
        });

        return app;
    }
}

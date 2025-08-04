using FloodOnlineReportingTool.Public.Models.OpenApi;
using Scalar.AspNetCore;

namespace Microsoft.AspNetCore.Builder;

internal static class OpenApiExtensions
{
    /// <summary>
    /// Add OpenApi using the versions
    /// </summary>
    /// <remarks>
    ///     <para>See: <see href="https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/customize-openapi?view=aspnetcore-9.0#use-document-transformers">Use document transformers</see></para>
    /// </remarks>
    internal static IServiceCollection AddFloodReportingOpenApi(this IServiceCollection services)
    {
        foreach (var versionInfo in Versions.All)
        {
            services.AddOpenApi(versionInfo.DocumentName, options =>
            {
                // Set the OpenAPI document info
                options.AddDocumentTransformer(new DocumentTransformer(versionInfo.Version));

                // Add API methods to the correct versions by using the group name
                options.ShouldInclude = (description) =>
                    description.GroupName == null ||
                    description.GroupName.Equals(options.DocumentName, StringComparison.OrdinalIgnoreCase);
            });
        }

        return services;
    }

    /// <summary>
    /// Add the Swagger UI <see href="https://localhost:7222/report-flooding/swagger" />
    /// </summary>
    internal static WebApplication UseSwagger(this WebApplication app)
    {
        app.UseSwaggerUI(options =>
        {
            options.DocumentTitle = DocumentTransformer.Title;
            options.EnableTryItOutByDefault();
            foreach (var versionInfo in Versions.All)
            {
                options.SwaggerEndpoint($"/openapi/{versionInfo.DocumentName}.json", $"{DocumentTransformer.Title} {versionInfo.DocumentName}");
            }
            options.OAuthUsePkce();
        });

        return app;
    }

    /// <summary>
    /// Add the Scalar UI <see href="https://localhost:7222/report-flooding/scalar" />
    /// </summary>
    internal static WebApplication UseScalar(this WebApplication app)
    {
        var documentNames = Versions.All.Select(o => o.DocumentName);

        app.MapScalarApiReference(options =>
        {
            options
                .WithTitle(DocumentTransformer.Title)
                .WithOperationSorter(OperationSorter.Alpha)
                .AddDocuments(documentNames)
                .AddPreferredSecuritySchemes("Bearer")
                .AddOAuth2Flows("Bearer", flows =>
                {
                    flows.WithAuthorizationCode(options =>
                    {
                        options.Pkce = Pkce.Sha256;
                    });
                });
        });

        return app;
    }
}

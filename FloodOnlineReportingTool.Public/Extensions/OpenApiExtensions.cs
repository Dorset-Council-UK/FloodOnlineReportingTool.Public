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
    /// Add the Swagger UI <see href="https://localhost:7222/swagger" />
    /// </summary>
    internal static WebApplication MapSwagger(this WebApplication app)
    {
        app.UseSwaggerUI(options =>
        {
            options.DocumentTitle = DocumentTransformer.Title;
            options.EnableTryItOutByDefault();
            foreach (var versionInfo in Versions.All)
            {
                options.SwaggerEndpoint($"/openapi/{versionInfo.DocumentName}.json", $"{DocumentTransformer.Title} {versionInfo.DocumentName}");
            }
        });

        return app;
    }

    /// <summary>
    /// Add the Scalar UI <see href="https://localhost:7222/scalar" />
    /// </summary>
    internal static WebApplication MapScalar(this WebApplication app)
    {
        const string BearerAuth = "BearerAuth";

        app.MapScalarApiReference(options =>
        {
            options
                .WithTitle(DocumentTransformer.Title)
                .WithOperationSorter(OperationSorter.Alpha)
                .WithOpenApiRoutePattern($"/openapi/{Versions.V1.DocumentName}.json")
                .WithModels(false)
                .WithPreferredScheme(BearerAuth)
                .AddHttpAuthentication(BearerAuth, scheme =>
                {
                    scheme.Token = "your-bearer-token";
                });
        });

        return app;
    }
}

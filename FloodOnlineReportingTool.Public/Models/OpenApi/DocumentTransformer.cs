using Asp.Versioning;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace FloodOnlineReportingTool.Public.Models.OpenApi;

internal sealed class DocumentTransformer(ApiVersion version) : IOpenApiDocumentTransformer
{
    internal static string Title = "Flood Reporting API";
    internal static string Description = "Flood reporting API and application";

    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Info = new()
        {
            Version = version.ToString("VVVV", ApiVersionFormatProvider.CurrentCulture),
            Title = Title,
            Description = Description,
            TermsOfService = new("https://example.com/terms"),
            Contact = new()
            {
                Name = "Contact",
                Url = new("https://example.com/contact"),
            },
            License = new()
            {
                Name = "MIT",
                Url = new("https://opensource.org/license/mit"),
            },
        };

        return Task.CompletedTask;
    }
}

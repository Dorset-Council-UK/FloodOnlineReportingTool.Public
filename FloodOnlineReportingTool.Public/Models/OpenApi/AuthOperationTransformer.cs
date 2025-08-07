using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace FloodOnlineReportingTool.Public.Models.OpenApi;

internal sealed class AuthOperationTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        var unauthorized = StatusCodes.Status401Unauthorized.ToString();
        if (!operation.Responses.ContainsKey(unauthorized))
        {
            operation.Responses.Add(unauthorized, new() { Description = "Unauthorized" });
        }

        var forbidden = StatusCodes.Status403Forbidden.ToString();
        if (!operation.Responses.ContainsKey(forbidden))
        {
            operation.Responses.Add(forbidden, new() { Description = "Forbidden" });
        }

        return Task.CompletedTask;
    }
}

using FloodOnlineReportingTool.Public.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FloodOnlineReportingTool.Public.Endpoints;

internal static class MediaEndpoints
{
    internal static async Task<Results<FileStreamHttpResult, NotFound>> GetMedia(string blobName, IBlobStorageService blobStorageService, CancellationToken cancellationToken)
    {
        var decodedBlobName = Uri.UnescapeDataString(blobName);
        var result = await blobStorageService.OpenReadAsync(decodedBlobName, cancellationToken);
        if (result is null)
        {
            return TypedResults.NotFound();
        }

        // enableRangeProcessing allows video seeking
        return TypedResults.Stream(result.Value.Content, result.Value.ContentType, enableRangeProcessing: true);
    }
}

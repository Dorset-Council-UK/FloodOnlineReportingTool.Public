using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.Public.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace FloodOnlineReportingTool.Public.Endpoints;

internal static class MediaEndpoints
{
    internal static async Task<Results<FileStreamHttpResult, NotFound, UnauthorizedHttpResult>> GetMedia(
        Guid floodReportSourceId,
        string blobName,
        ClaimsPrincipal user,
        IFloodReportSourceRepository floodReportSourceRepository,
        IBlobStorageService blobStorageService,
        CancellationToken cancellationToken)
    {
        var userId = user.Oid;
        if (userId is null)
        {
            return TypedResults.Unauthorized();
        }

        var floodReportSourceExists = await floodReportSourceRepository.Exists(userId, floodReportSourceId, cancellationToken);
        if (!floodReportSourceExists)
        {
            return TypedResults.NotFound();
        }

        if (string.IsNullOrWhiteSpace(blobName))
        {
            return TypedResults.NotFound();
        }
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

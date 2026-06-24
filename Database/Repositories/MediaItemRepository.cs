using FloodOnlineReportingTool.Database.DbContexts;
using FloodOnlineReportingTool.Database.Models;
using FloodOnlineReportingTool.Database.Models.Flood;
using FloodOnlineReportingTool.Database.Models.ResultModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FloodOnlineReportingTool.Database.Repositories;

public class MediaItemRepository(
    ILogger<MediaItemRepository> logger,
    IDbContextFactory<PublicDbContext> contextFactory
) : IMediaItemRepository
{
    public async Task<int> GetCountByReport(Guid floodReportId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting media item count for flood report ID: {FloodReportId}", floodReportId);

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.FloodReportSources
            .AsNoTracking()
            .IgnoreAutoIncludes()
            .Where(fr => fr.Id == floodReportId)
            .Select(fr => fr.MediaItems.Count)
            .SingleOrDefaultAsync(cancellationToken);
    }
    public async Task<IReadOnlyCollection<MediaItem>> GetByReport(Guid floodReportId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting media items for flood report ID: {FloodReportId}", floodReportId);

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var floodReport = await context.FloodReportSources
            .AsNoTracking()
            .IgnoreAutoIncludes()
            .Include(fr => fr.MediaItems)
            .FirstOrDefaultAsync(fr => fr.Id == floodReportId, cancellationToken);

        var mediaItems = floodReport?.MediaItems
            .OrderByDescending(mi => mi.UploadDateUtc)
            .ToList();

        return mediaItems ?? [];
    }

    public async Task<Result<IReadOnlyCollection<MediaItem>>> Create(Guid floodReportId, IReadOnlyCollection<MediaItem> mediaItems, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating media items for flood report ID: {FloodReportId}", floodReportId);

        if (floodReportId == Guid.Empty)
        {
            logger.LogInformation("Empty flood report ID provided for media items");
            return Result<IReadOnlyCollection<MediaItem>>.Failure(["Cannot create media items with empty flood report ID"]);
        }

        if (mediaItems.Count == 0)
        {
            logger.LogInformation("No media items provided for flood report ID: {FloodReportId}", floodReportId);
            return Result<IReadOnlyCollection<MediaItem>>.Success([]);
        }

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        FloodReportSource? floodReport = await context.FloodReportSources
            .Include(fr => fr.MediaItems)
            .FirstOrDefaultAsync(fr => fr.Id == floodReportId, cancellationToken);

        if (floodReport is null)
        {
            logger.LogInformation("Flood report {FloodReportId} not found for media items", floodReportId);
            return Result<IReadOnlyCollection<MediaItem>>.Failure([$"Cannot create media items, no flood report found for ID {floodReportId}"]);
        }

        var createdMediaItems = mediaItems
            .Select(mediaItem => new MediaItem
            {
                Id = mediaItem.Id,
                UploadDateUtc = mediaItem.UploadDateUtc ?? DateTimeOffset.UtcNow,
                URL = mediaItem.URL,
                Title = mediaItem.Title,
            })
            .ToList();

        foreach (var mediaItem in createdMediaItems)
        {
            floodReport.MediaItems.Add(mediaItem);
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result<IReadOnlyCollection<MediaItem>>.Success(createdMediaItems);
    }

    public async Task<DeleteResult<MediaItem>> Delete(Guid mediaItemId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting media item ID: {MediaItemId}", mediaItemId);

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var mediaItem = await context.MediaItems
            .FirstOrDefaultAsync(mi => mi.Id == mediaItemId, cancellationToken);

        if (mediaItem is null)
        {
            return DeleteResult<MediaItem>.Failure([$"An error occurred deleting the media item"]);
        }

        context.MediaItems.Remove(mediaItem);
        await context.SaveChangesAsync(cancellationToken);

        return DeleteResult<MediaItem>.Success();
    }

    public async Task<Result<MediaItem>> UpdateTitle(Guid mediaItemId, string title, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating title for media item ID: {MediaItemId}", mediaItemId);

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var mediaItem = await context.MediaItems
            .FirstOrDefaultAsync(mi => mi.Id == mediaItemId, cancellationToken);

        if (mediaItem is null)
        {
            logger.LogWarning(message: "Couldn't rename media item. No media item found for ID {mediaItemId}", mediaItemId);
            return Result<MediaItem>.Failure([$"An error occurred renaming the media item"]);
        }

        mediaItem.Title = title;
        await context.SaveChangesAsync(cancellationToken);

        return Result<MediaItem>.Success(mediaItem);
    }
}

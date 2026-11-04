using FloodOnlineReportingTool.Database.DbContexts;
using FloodOnlineReportingTool.Database.Models;
using FloodOnlineReportingTool.Database.Models.Flood;
using FloodOnlineReportingTool.Database.Models.ResultModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FloodOnlineReportingTool.Database.Repositories;

public class MediaItemRepository(
    ILogger<MediaItemRepository> logger,
    IDbContextFactory<PublicDbContext> contextFactory
) : IMediaItemRepository
{
    public async Task<int> GetCountByReport(Guid floodReportSourceId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting media item count for flood report source ID: {FloodReportSourceId}", floodReportSourceId);

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.FloodReportSources
            .AsNoTracking()
            .IgnoreAutoIncludes()
            .Where(frs => frs.Id == floodReportSourceId)
            .Select(frs => frs.MediaItems.Count)
            .SingleOrDefaultAsync(cancellationToken);
    }
    public async Task<IReadOnlyCollection<MediaItem>> GetByReport(Guid floodReportSourceId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting media items for flood report source ID: {FloodReportSourceId}", floodReportSourceId);

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var floodReportSource = await context.FloodReportSources
            .AsNoTracking()
            .IgnoreAutoIncludes()
            .Include(frs => frs.MediaItems)
            .FirstOrDefaultAsync(frs => frs.Id == floodReportSourceId, cancellationToken);

        var mediaItems = floodReportSource?.MediaItems
            .OrderByDescending(mi => mi.UploadDateUtc)
            .ToList();

        return mediaItems ?? [];
    }

    public async Task<Result<IReadOnlyCollection<MediaItem>>> Create(Guid floodReportSourceId, IReadOnlyCollection<MediaItem> mediaItems, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating media items for flood report source ID: {FloodReportSourceId}", floodReportSourceId);

        if (floodReportSourceId == Guid.Empty)
        {
            logger.LogInformation("Empty flood report source ID provided for media items");
            return Result<IReadOnlyCollection<MediaItem>>.Failure(["Cannot create media items with empty flood report source ID"]);
        }

        if (mediaItems.Count == 0)
        {
            logger.LogInformation("No media items provided for flood report source ID: {FloodReportSourceId}", floodReportSourceId);
            return Result<IReadOnlyCollection<MediaItem>>.Success([]);
        }

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        FloodReportSource? floodReportSource = await context.FloodReportSources
            .Include(frs => frs.MediaItems)
            .FirstOrDefaultAsync(frs => frs.Id == floodReportSourceId, cancellationToken);

        if (floodReportSource is null)
        {
            logger.LogInformation("Flood report source {FloodReportSourceId} not found for media items", floodReportSourceId);
            return Result<IReadOnlyCollection<MediaItem>>.Failure([$"Cannot create media items, no flood report source found for ID {floodReportSourceId}"]);
        }

        foreach (var mediaItem in mediaItems)
        {
            mediaItem.UploadDateUtc ??= DateTimeOffset.UtcNow;
            floodReportSource.MediaItems.Add(mediaItem);
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result<IReadOnlyCollection<MediaItem>>.Success(mediaItems);
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

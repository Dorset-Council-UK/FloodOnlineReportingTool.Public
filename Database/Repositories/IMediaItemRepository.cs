using FloodOnlineReportingTool.Database.Models;
using FloodOnlineReportingTool.Database.Models.ResultModels;

namespace FloodOnlineReportingTool.Database.Repositories;

public interface IMediaItemRepository
{
    /// <summary>
    /// Gets a count of all media items associated with a flood report source.
    /// </summary>
    /// <returns>Integer indicating total number of media items</returns>
    Task<int> GetCountByReport(Guid floodReportSourceId, CancellationToken cancellationToken);
    /// <summary>
    /// Get all media items associated with a flood report source.
    /// </summary>
    Task<IReadOnlyCollection<MediaItem>> GetByReport(Guid floodReportSourceId, CancellationToken cancellationToken);

    /// <summary>
    /// Create media items and associate them with a flood report source.
    /// </summary>
    /// <returns>A result pattern with the created media items, or a list of errors.</returns>
    Task<Result<IReadOnlyCollection<MediaItem>>> Create(Guid floodReportSourceId, IReadOnlyCollection<MediaItem> mediaItems, CancellationToken cancellationToken);

    /// <summary>
    /// Update the title of a media item.
    /// </summary>
    /// <returns>A result pattern with the updated media item, or a list of errors.</returns>
    Task<Result<MediaItem>> UpdateTitle(Guid mediaItemId, string title, CancellationToken cancellationToken);

    /// <summary>
    /// Delete a media item by ID.
    /// </summary>
    /// <returns>A result pattern indicating success, or a list of errors.</returns>
    Task<DeleteResult<MediaItem>> Delete(Guid mediaItemId, CancellationToken cancellationToken);
}

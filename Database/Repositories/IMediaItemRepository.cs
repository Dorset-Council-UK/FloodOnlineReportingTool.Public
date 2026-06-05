using FloodOnlineReportingTool.Database.Models;
using FloodOnlineReportingTool.Database.Models.ResultModels;

namespace FloodOnlineReportingTool.Database.Repositories;

public interface IMediaItemRepository
{
    /// <summary>
    /// Get all media items associated with a flood report.
    /// </summary>
    Task<IReadOnlyCollection<MediaItem>> GetByReport(Guid floodReportId, CancellationToken cancellationToken);

    /// <summary>
    /// Create media items and associate them with a flood report.
    /// </summary>
    /// <returns>A result pattern with the created media items, or a list of errors.</returns>
    Task<Result<IReadOnlyCollection<MediaItem>>> Create(Guid floodReportId, IReadOnlyCollection<MediaItem> mediaItems, CancellationToken cancellationToken);

    /// <summary>
    /// Delete a media item by ID.
    /// </summary>
    /// <returns>A result pattern indicating success, or a list of errors.</returns>
    Task<DeleteResult<MediaItem>> Delete(Guid mediaItemId, CancellationToken cancellationToken);
}

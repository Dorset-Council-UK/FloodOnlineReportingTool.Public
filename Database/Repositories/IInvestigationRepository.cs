using FloodOnlineReportingTool.Database.Models.Investigate;

namespace FloodOnlineReportingTool.Database.Repositories;

public interface IInvestigationRepository
{
    /// <summary>
    /// Get the investigation for the given user, going via the flood report
    /// </summary>
    Task<Investigation?> ReportedByUser(string userId, Guid id, CancellationToken ct);

    /// <summary>
    /// Create the investigation for the given user, going via the flood report
    /// </summary>
    Task<Investigation> CreateForUser(string userId, InvestigationDto dto, CancellationToken ct);

    /// <summary>
    /// Get basic investigation information for the given user. (no related records)
    /// </summary>
    Task<Investigation?> ReportedByUserBasicInformation(string userId, CancellationToken ct);
}

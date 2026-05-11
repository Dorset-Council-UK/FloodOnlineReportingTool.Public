using FloodOnlineReportingTool.Database.Models.Investigate;
using FloodOnlineReportingTool.Database.Models.ResultModels;

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
    /// <returns>A result pattern with the created investigation, or a list of errors.</returns>
    Task<Result<Investigation>> CreateForFloodReport(string userId, InvestigationDto dto, CancellationToken ct);

    /// <summary>
    /// Get basic investigation information for the given user. (no related records)
    /// </summary>
    Task<Investigation?> ReportedByUserBasicInformation(string userId, CancellationToken ct);
}

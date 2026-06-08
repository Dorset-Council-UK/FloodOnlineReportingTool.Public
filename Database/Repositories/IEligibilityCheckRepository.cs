using FloodOnlineReportingTool.Database.Models.Eligibility;

namespace FloodOnlineReportingTool.Database.Repositories;

public interface IEligibilityCheckRepository
{
    /// <summary>
    /// Get the eligibility check reported by the user
    /// </summary>
    Task<EligibilityCheck?> ReportedByUser(string userId, CancellationToken ct);

    /// <summary>
    /// Get the eligibility check reported by the user
    /// </summary>
    Task<EligibilityCheck?> ReportedByUser(string userId, Guid eligibilityCheckId, CancellationToken ct);

    Task<EligibilityCheck?> GetById(Guid id, CancellationToken ct);
    Task<EligibilityCheck?> GetByReference(string reference, CancellationToken ct);
}

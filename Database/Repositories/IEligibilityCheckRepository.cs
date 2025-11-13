using FloodOnlineReportingTool.Database.Models.Eligibility;

namespace FloodOnlineReportingTool.Database.Repositories;

public interface IEligibilityCheckRepository
{
    /// <summary>
    /// Get the eligibility check reported by the user
    /// </summary>
    Task<EligibilityCheck?> ReportedByUser(Guid userId, CancellationToken ct);

    /// <summary>
    /// Get the eligibility check reported by the user
    /// </summary>
    Task<EligibilityCheck?> ReportedByUser(Guid userId, Guid id, CancellationToken ct);

    Task<EligibilityCheck?> GetById(Guid id, CancellationToken ct);
    Task<EligibilityCheck?> GetByReference(string reference, CancellationToken ct);
    Task<EligibilityCheck?> Update(Guid id, EligibilityCheckDto dto, CancellationToken ct);

    /// <summary>
    /// Update the eligibility check reported by the user
    /// </summary>
    Task<EligibilityCheck> UpdateForUser(Guid userId, Guid id, EligibilityCheckDto dto, CancellationToken ct);
}

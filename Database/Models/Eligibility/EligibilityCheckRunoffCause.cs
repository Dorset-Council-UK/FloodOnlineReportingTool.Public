using FloodOnlineReportingTool.Database.Models.Flood;

namespace FloodOnlineReportingTool.Database.Models.Eligibility;

/// <summary>
/// Represents the one-to-many relationship between an eligibility check and secondary cause flood problems.
/// </summary>
public record EligibilityCheckRunoffCause(Guid EligibilityCheckId, Guid FloodProblemId)
{
    public Guid EligibilityCheckId { get; init; } = EligibilityCheckId;
    public Guid FloodProblemId { get; init; } = FloodProblemId;
    public FloodProblem FloodProblem { get; init; } = null!;

}

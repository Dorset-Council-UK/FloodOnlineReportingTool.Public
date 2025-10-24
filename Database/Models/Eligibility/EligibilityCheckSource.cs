using FloodOnlineReportingTool.Database.Models.Flood;

namespace FloodOnlineReportingTool.Database.Models.Eligibility;

/// <summary>
/// Represents the one-to-many relationship between an eligibility check and source flood problems.
/// </summary>
public record EligibilityCheckSource(Guid EligibilityCheckId, Guid FloodProblemId)
{
    public Guid EligibilityCheckId { get; init; } = EligibilityCheckId;
    public Guid FloodProblemId { get; init; } = FloodProblemId;
    public FloodProblem FloodProblem { get; init; } = null!;

}

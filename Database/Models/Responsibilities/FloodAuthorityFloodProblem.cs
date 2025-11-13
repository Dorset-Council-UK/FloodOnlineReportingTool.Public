using FloodOnlineReportingTool.Database.Models.Flood;

namespace FloodOnlineReportingTool.Database.Models.Responsibilities;

/// <summary>
/// Represents the many-to-many relationship between a flood authority and a flood problem.
/// This entity links a FloodAuthority to multiple FloodProblems and vice versa.
/// </summary>
public record FloodAuthorityFloodProblem(Guid FloodAuthorityId, Guid FloodProblemId)
{
    public Guid FloodAuthorityId { get; init; } = FloodAuthorityId;
    public Guid FloodProblemId { get; init; } = FloodProblemId;
    public FloodProblem FloodProblem { get; init; } = null!;
}

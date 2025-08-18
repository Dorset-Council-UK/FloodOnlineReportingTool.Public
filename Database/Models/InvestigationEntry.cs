namespace FloodOnlineReportingTool.Database.Models;

/// <summary>
/// Represents the one-to-many relationship between one investigation and many water entries, which are flood problems.
/// </summary>
public record InvestigationEntry(Guid InvestigationId, Guid FloodProblemId)
{
    public Guid InvestigationId { get; init; } = InvestigationId;
    public Investigation Investigation { get; init; } = null!;
    public Guid FloodProblemId { get; init; } = FloodProblemId;
    public FloodProblem FloodProblem { get; init; } = null!;
}

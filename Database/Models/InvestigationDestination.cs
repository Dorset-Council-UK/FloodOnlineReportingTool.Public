namespace FloodOnlineReportingTool.Database.Models;

/// <summary>
/// Represents the one-to-many relationship between one investigation and many destinations, which are flood problems.
/// </summary>
public record InvestigationDestination(Guid InvestigationId, Guid FloodProblemId)
{
    public Guid InvestigationId { get; init; } = InvestigationId;
    public Guid FloodProblemId { get; init; } = FloodProblemId;
    public FloodProblem FloodProblem { get; init; } = null!;
}

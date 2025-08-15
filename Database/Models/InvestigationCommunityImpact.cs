namespace FloodOnlineReportingTool.Database.Models;

/// <summary>
/// Represents the one-to-many relationship between one investigation and many community impacts, which are flood impacts
/// </summary>
public record InvestigationCommunityImpact(Guid InvestigationId, Guid FloodImpactId)
{
    public Guid InvestigationId { get; init; } = InvestigationId;
    public Guid FloodImpactd { get; init; } = FloodImpactId;
    public FloodImpact FloodImpact { get; init; } = null!;
}

using FloodOnlineReportingTool.Database.Models.Flood;

namespace FloodOnlineReportingTool.Database.Models.Investigate;

/// <summary>
/// Represents the one-to-many relationship between one investigation and many community impacts, which are flood impacts
/// </summary>
public record InvestigationCommunityImpact(Guid InvestigationId, Guid FloodImpactId)
{
    public Guid InvestigationId { get; init; } = InvestigationId;
    public Guid FloodImpactId { get; init; } = FloodImpactId;
    public FloodImpact FloodImpact { get; init; } = null!;
}

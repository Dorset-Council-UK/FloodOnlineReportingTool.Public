using FloodOnlineReportingTool.Database.Models.Flood;

namespace FloodOnlineReportingTool.Database.Models.Eligibility;

/// <summary>
/// Represents the one-to-many relationship between an eligibility check and residential flood impacts.
/// </summary>
public record EligibilityCheckResidential(Guid EligibilityCheckId, Guid FloodImpactId)
{
    public Guid EligibilityCheckId { get; init; } = EligibilityCheckId;
    public Guid FloodImpactId { get; init; } = FloodImpactId;
    public FloodImpact FloodImpact { get; init; } = null!;
}

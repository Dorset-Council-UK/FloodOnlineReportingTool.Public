namespace FloodOnlineReportingTool.Database.Models;

/// <summary>
/// Represents the one-to-many relationship between one investigation and many help received options, which are flood mitigations.
/// </summary>
public record InvestigationHelpReceived(Guid InvestigationId, Guid FloodMitigationId)
{
    public Guid InvestigationId { get; init; } = InvestigationId;
    public Guid FloodMitigationId { get; init; } = FloodMitigationId;
    public FloodMitigation FloodMitigation { get; init; } = null!;
}

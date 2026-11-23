using FloodOnlineReportingTool.Database.Models.Status;

namespace FloodOnlineReportingTool.Database.Models.Investigate;

public record InvestigationPeakDepth
{
    public Guid InvestigationId { get; init; }
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public DateTimeOffset CreatedUtc { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedUtc { get; init; }
    public required Guid IsPeakDepthKnownId { get; init; }
    public virtual RecordStatus IsPeakDepthKnown { get; init; } = default!;
    public int? PeakCentimetres { get; init; }
    public required string DepthLocation { get; init; }
}
using FloodOnlineReportingTool.Database.Models.Status;
using System;
using System.Collections.Generic;
using System.Text;

namespace FloodOnlineReportingTool.Database.Models.Investigate
{
    public record InvestigationPeakDepth(Guid InvestigationId, Guid IsPeakDepthKnownId, int? PeakInsideCentimetres, int? PeakOutsideCentimetres)
    {
        public Guid InvestigationId { get; init; } = InvestigationId;
        public Guid Id { get; init; } = Guid.CreateVersion7();
        public DateTimeOffset CreatedUtc { get; init; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedUtc { get; init; }
        public required Guid IsPeakDepthKnownId { get; init; } = IsPeakDepthKnownId;
        public RecordStatus IsPeakDepthKnown { get; init; } = default!;
        public int? PeakInsideCentimetres { get; init; } = PeakInsideCentimetres;
        public int? PeakOutsideCentimetres { get; init; } = PeakOutsideCentimetres;
    }
}
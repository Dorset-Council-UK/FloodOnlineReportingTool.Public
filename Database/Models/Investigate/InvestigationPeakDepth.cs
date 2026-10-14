using FloodOnlineReportingTool.Database.Models.Status;
using System;
using System.Collections.Generic;
using System.Text;

namespace FloodOnlineReportingTool.Database.Models.Investigate
{
    public record InvestigationPeakDepth
    {
        public Guid Id { get; init; } = Guid.CreateVersion7();
        public DateTimeOffset CreatedUtc { get; init; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedUtc { get; init; }
        public required Guid IsPeakDepthKnownId { get; init; }
        public RecordStatus IsPeakDepthKnown { get; init; } = default!;
        public int? PeakInsideCentimetres { get; init; }
        public int? PeakOutsideCentimetres { get; init; }
    }
}

using FloodOnlineReportingTool.Contracts;

namespace FloodOnlineReportingTool.Database.Models.Flood;

internal static class FloodExtensions
{
    private const int defaultBufferSize = 25;

    extension(FloodImpact? floodImpact)
    {
        internal bool IsInternal
        {
            get
            {
                if (string.IsNullOrWhiteSpace(floodImpact?.CategoryPriority))
                {
                    return false;
                }

                return floodImpact.CategoryPriority.Equals(FloodImpactPriority.Internal, StringComparison.OrdinalIgnoreCase);
            }
        }

        internal bool IsExternal
        {
            get
            {
                if (string.IsNullOrWhiteSpace(floodImpact?.CategoryPriority))
                {
                    return false;
                }

                return floodImpact.CategoryPriority.Equals(FloodImpactPriority.External, StringComparison.OrdinalIgnoreCase);
            }
        }
    }

    extension(FloodReport floodReport)
    {
        internal FloodReportSourceCreated ToMessageCreated(Uri baseViewUri, EligibilityCheckRecord eligibilityCheckRecord)
        {
            var floodReportViewUri = new Uri($"{baseViewUri.AbsoluteUri.TrimEnd('/')}/{Uri.EscapeDataString(floodReport.Reference)}");

            return new FloodReportSourceCreated(
                floodReport.Id,
                defaultBufferSize,
                floodReport.Reference,
                floodReportViewUri,
                floodReport.CreatedUtc,
                eligibilityCheckRecord,
                floodReport.Investigation is not null,
                floodReport.ContactRecords.Count > 0,
                [.. floodReport.ContactRecords
                    .SelectMany(c => c.SubscribeRecords)
                    .Select(s => s.ContactType)
                    .Distinct(),
                ]
            );
        }
    }
}

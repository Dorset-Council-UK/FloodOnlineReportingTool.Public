using FloodOnlineReportingTool.Contracts;

namespace FloodOnlineReportingTool.Database.Models.Flood;

internal static class FloodExtensions
{
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
        internal FloodReportSourceCreated ToMessageCreated(Uri uri, EligibilityCheckRecord eligibilityCheckRecord)
        {
            return new FloodReportSourceCreated(
                floodReport.Id,
                25,
                floodReport.Reference,
                new Uri(uri, floodReport.Reference),
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

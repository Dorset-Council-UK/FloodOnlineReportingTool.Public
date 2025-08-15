using FloodOnlineReportingTool.Contracts;

namespace FloodOnlineReportingTool.Database.Models;

public static class FloodReportExtensions
{
    internal static FloodReportCreated ToMessageCreated(this FloodReport floodReport)
    {
        return new FloodReportCreated(
            floodReport.Id,
            floodReport.Reference,
            floodReport.CreatedUtc,
            floodReport.EligibilityCheck is not null,
            floodReport.Investigation is not null,
            floodReport.ContactRecords.Count
        );
    }
}

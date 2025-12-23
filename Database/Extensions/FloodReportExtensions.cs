using FloodOnlineReportingTool.Contracts;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace FloodOnlineReportingTool.Database.Models.Flood;
#pragma warning restore IDE0130 // Namespace does not match folder structure

internal static class FloodReportExtensions
{
    internal static FloodReportCreated ToMessageCreated(this FloodReport floodReport)
    {
        return new FloodReportCreated(
            floodReport.Id,
            floodReport.Reference,
            floodReport.CreatedUtc,
            floodReport.EligibilityCheck is not null,
            floodReport.Investigation is not null,
            floodReport.ContactRecords.Count > 0,
            [.. floodReport.ContactRecords
                .SelectMany(c => c.SubscribeRecords)
                .Select(s => s.ContactType)
                .Distinct()]
        );
    }
}

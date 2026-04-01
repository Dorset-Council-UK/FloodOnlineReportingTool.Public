using FloodOnlineReportingTool.Database.Models.Eligibility;

namespace FloodOnlineReportingTool.Public.Models.FloodReport.Overview;

internal static class UpdateModelExtensions
{
    internal static EligibilityCheckDto ToDto(this UpdateModel updateModel)
    {
        return new()
        {
            Uprn = updateModel.UprnNumber!.Value,
            Easting = updateModel.EastingNumber!.Value,
            Northing = updateModel.NorthingNumber!.Value,
            LocationDesc = updateModel.LocationDesc,
        };
    }
}

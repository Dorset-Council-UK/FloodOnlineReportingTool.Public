#pragma warning disable IDE0130 // Namespace does not match folder structure
using FloodOnlineReportingTool.Database.Models.Eligibility;

namespace FloodOnlineReportingTool.Public.Models.FloodReport.Update;
#pragma warning restore IDE0130 // Namespace does not match folder structure

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

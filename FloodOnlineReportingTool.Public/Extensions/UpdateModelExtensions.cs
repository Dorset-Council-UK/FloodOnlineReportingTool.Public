using FloodOnlineReportingTool.DataAccess.Models;

namespace FloodOnlineReportingTool.Public.Models.FloodReport.Update;

internal static class UpdateModelExtensions
{
    internal static EligibilityCheckDto ToDto(this UpdateModel updateModel)
    {
        return new()
        {
            Uprn = updateModel.UprnNumber.Value,
            Easting = updateModel.EastingNumber.Value,
            Northing = updateModel.NorthingNumber.Value,
            LocationDesc = updateModel.LocationDesc,
        };
    }
}

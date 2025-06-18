using FloodOnlineReportingTool.DataAccess.Models;

namespace FloodOnlineReportingTool.Public.Models.FloodReport.Update;

internal static class UpdateModelExtensions
{
    internal static EligibilityCheckDto ToDto(this UpdateModel updateModel)
    {
        return new()
        {
            Uprn = updateModel.Uprn,
            Easting = updateModel.Easting,
            Northing = updateModel.Northing,
            LocationDesc = updateModel.LocationDesc,
        };
    }
}

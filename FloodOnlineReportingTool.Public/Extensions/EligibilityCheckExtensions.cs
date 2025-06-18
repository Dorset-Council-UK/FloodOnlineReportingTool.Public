using FloodOnlineReportingTool.Public.Models.FloodReport.Update;

namespace FloodOnlineReportingTool.DataAccess.Models;

internal static class EligibilityCheckExtensions
{
    internal static UpdateModel ToUpdateModel(this EligibilityCheck eligibilityCheck)
    {
        return new()
        {
            Id = eligibilityCheck.Id,
            CreatedUtc = eligibilityCheck.CreatedUtc,
            UpdatedUtc = eligibilityCheck.UpdatedUtc,
            Uprn = eligibilityCheck.Uprn,
            Easting = eligibilityCheck.Easting,
            Northing = eligibilityCheck.Northing,
            LocationDesc = eligibilityCheck.LocationDesc,
        };
    }
}

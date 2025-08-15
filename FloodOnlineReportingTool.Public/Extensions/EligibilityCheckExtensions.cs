using FloodOnlineReportingTool.Public.Models.FloodReport.Update;
using System.Globalization;

namespace FloodOnlineReportingTool.Database.Models;

internal static class EligibilityCheckExtensions
{
    internal static UpdateModel ToUpdateModel(this EligibilityCheck eligibilityCheck)
    {
        return new()
        {
            Id = eligibilityCheck.Id,
            CreatedUtc = eligibilityCheck.CreatedUtc,
            UpdatedUtc = eligibilityCheck.UpdatedUtc,
            UprnNumber = (int)eligibilityCheck.Uprn,
            UprnText = eligibilityCheck.Uprn.ToString(CultureInfo.CurrentCulture),
            EastingNumber = (float)eligibilityCheck.Easting,
            EastingText = eligibilityCheck.Easting.ToString(CultureInfo.CurrentCulture),
            NorthingNumber = (float)eligibilityCheck.Northing,
            NorthingText = eligibilityCheck.Northing.ToString(CultureInfo.CurrentCulture),
            LocationDesc = eligibilityCheck.LocationDesc,
        };
    }
}

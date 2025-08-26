using FloodOnlineReportingTool.Public.Models.FloodReport.Update;
using System.Globalization;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace FloodOnlineReportingTool.Database.Models;
#pragma warning restore IDE0130 // Namespace does not match folder structure

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

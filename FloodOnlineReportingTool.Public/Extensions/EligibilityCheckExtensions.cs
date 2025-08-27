using FloodOnlineReportingTool.Public.Models.FloodReport.Update;
using System.Globalization;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace FloodOnlineReportingTool.Database.Models;
#pragma warning restore IDE0130 // Namespace does not match folder structure

internal static class EligibilityCheckExtensions
{
    internal static UpdateModel ToUpdateModel(this EligibilityCheck eligibilityCheck)
    {
        // convert the long to an int for UPRN number
        var uprnNumber = eligibilityCheck.Uprn is >= int.MinValue and <= int.MaxValue
            ? (int?)eligibilityCheck.Uprn
            : null;

        // convert the double to a float for northing number
        var northingNumber = eligibilityCheck.Northing is >= float.MinValue and <= float.MaxValue
            ? (float?)eligibilityCheck.Northing
            : null;

        // convert the double to a float for easting number
        var eastingNumber = eligibilityCheck.Easting is >= float.MinValue and <= float.MaxValue
            ? (float?)eligibilityCheck.Easting
            : null;

        return new()
        {
            Id = eligibilityCheck.Id,
            CreatedUtc = eligibilityCheck.CreatedUtc,
            UpdatedUtc = eligibilityCheck.UpdatedUtc,
            UprnNumber = uprnNumber,
            UprnText = eligibilityCheck.Uprn?.ToString(CultureInfo.CurrentCulture),
            EastingNumber = eastingNumber,
            EastingText = eligibilityCheck.Easting.ToString(CultureInfo.CurrentCulture),
            NorthingNumber = northingNumber,
            NorthingText = eligibilityCheck.Northing.ToString(CultureInfo.CurrentCulture),
            LocationDesc = eligibilityCheck.LocationDesc,
        };
    }
}

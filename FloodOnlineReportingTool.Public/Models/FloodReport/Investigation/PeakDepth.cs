using GdsBlazorComponents;
using System.Globalization;

namespace FloodOnlineReportingTool.Public.Models.FloodReport.Investigation;

public class PeakDepth
{
    [GdsFieldErrorClass(GdsFieldTypes.Radio)]
    public Guid? IsPeakDepthKnownId { get; set; }

    [GdsFieldErrorClass(GdsFieldTypes.Input)]
    public string? PeakCentimetresText { get; set; }
    public float? PeakCentimetresNumber => ToFloat(PeakCentimetresText);
    public int? PeakCentimetres => WholeNumberInt(PeakCentimetresNumber);
    public string PeakFeetAndInches => PeakCentimetres.ConvertMeasurementToDisplayString(StringExtensions.MeasurementDisplayType.FeetAndInches);

    [GdsFieldErrorClass(GdsFieldTypes.Input)]
    public string? DepthLocation { get; set; }

    private static float? ToFloat(string? value)
    {
        var toCheck = value?.Trim();
        if (toCheck == null)
        {
            return null;
        }
        return float.TryParse(toCheck, NumberStyles.Float, CultureInfo.InvariantCulture, out var result) ? result : null;
    }

    private static int? WholeNumberInt(float? value)
    {
        if (value == null)
        {
            return null;
        }

        return value.Value % 1 == 0 ? Convert.ToInt32(value.Value) : null;
    }
}

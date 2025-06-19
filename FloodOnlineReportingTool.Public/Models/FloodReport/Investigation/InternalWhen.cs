using GdsBlazorComponents;
using System.Globalization;

namespace FloodOnlineReportingTool.Public.Models.FloodReport.Investigation;

public class InternalWhen
{
    [GdsFieldErrorClass(GdsFieldTypes.Radio)]
    public Guid? WhenWaterEnteredKnownId { get; set; }

    [GdsFieldErrorClass(GdsFieldTypes.Date)]
    public GdsDate WhenWaterEnteredDate { get; set; } = new();

    [GdsFieldErrorClass(GdsFieldTypes.Input)]
    public string? TimeText { get; set; }
    public TimeOnly? Time => TimeOnly.TryParse(TimeText, CultureInfo.InvariantCulture, out var time) ? time : null;
}

using GdsBlazorComponents;

namespace FloodOnlineReportingTool.Public.Models.FloodReport.Create;

public class FloodDuration
{
    [GdsFieldErrorClass(GdsFieldTypes.Radio)]
    public Guid? DurationKnownId { get; set; }

    [GdsFieldErrorClass(GdsFieldTypes.Input)]
    public string? DurationDaysText { get; set; }
    public int? DurationDaysNumber { get; set; }

    [GdsFieldErrorClass(GdsFieldTypes.Input)]
    public string? DurationHoursText { get; set; }
    public int? DurationHoursNumber { get; set; }
}

using GdsBlazorComponents;

namespace FloodOnlineReportingTool.Public.Models.FloodReport.Create;

public class FloodDuration
{
    [GdsFieldErrorClass(GdsFieldTypes.Radio)]
    public Guid? DurationKnownId { get; set; }

    public string? DurationDaysText { get; set; }
    public int? DurationDaysNumber { get; set; }

    public string? DurationHoursText { get; set; }
    public int? DurationHoursNumber { get; set; }
}

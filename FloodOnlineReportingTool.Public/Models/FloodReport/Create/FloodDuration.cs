using FloodOnlineReportingTool.GdsComponents;

namespace FloodOnlineReportingTool.Public.Models.FloodReport.Create;

public class FloodDuration
{
    [GdsFieldErrorClass(GdsFieldTypes.Radio)]
    public Guid? DurationKnownId { get; set; }

    [GdsFieldErrorClass(GdsFieldTypes.Input)]
    public int? DurationDays { get; set; }

    [GdsFieldErrorClass(GdsFieldTypes.Input)]
    public int? DurationHours { get; set; }
}

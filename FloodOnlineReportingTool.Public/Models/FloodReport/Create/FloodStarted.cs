using FloodOnlineReportingTool.GdsComponents;

namespace FloodOnlineReportingTool.Public.Models.FloodReport.Create;

public class FloodStarted
{
    [GdsFieldErrorClass(GdsFieldTypes.Date)]
    public GdsDate StartDate { get; set; } = new();

    [GdsFieldErrorClass(GdsFieldTypes.Radio)]
    public bool? IsFloodOngoing { get; set; }
}

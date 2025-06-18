using FloodOnlineReportingTool.GdsComponents;

namespace FloodOnlineReportingTool.Public.Models.FloodReport.Investigation;

public class History
{
    [GdsFieldErrorClass(GdsFieldTypes.Radio)]
    public Guid? HistoryOfFloodingId { get; set; }

    [GdsFieldErrorClass(GdsFieldTypes.Textarea)]
    public string? HistoryOfFloodingDetails { get; set; }
}

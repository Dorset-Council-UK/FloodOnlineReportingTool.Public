using FloodOnlineReportingTool.GdsComponents;

namespace FloodOnlineReportingTool.Public.Models.FloodReport.Investigation;

public class Warnings
{
    [GdsFieldErrorClass(GdsFieldTypes.Radio)]
    public Guid? RegisteredWithFloodlineId { get; set; }

    [GdsFieldErrorClass(GdsFieldTypes.Radio)]
    public Guid? OtherWarningId { get; set; }
}

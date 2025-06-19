using GdsBlazorComponents;

namespace FloodOnlineReportingTool.Public.Models.FloodReport.Investigation;

public class Floodline
{
    [GdsFieldErrorClass(GdsFieldTypes.Radio)]
    public Guid? WarningTimelyId { get; set; }

    [GdsFieldErrorClass(GdsFieldTypes.Radio)]
    public Guid? WarningAppropriateId { get; set; }
}

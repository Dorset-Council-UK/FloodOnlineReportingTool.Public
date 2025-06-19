using GdsBlazorComponents;

namespace FloodOnlineReportingTool.Public.Models.FloodReport.Investigation;

public class Speed
{
    [GdsFieldErrorClass(GdsFieldTypes.Radio)]
    public Guid? Begin { get; set; }

    [GdsFieldErrorClass(GdsFieldTypes.Radio)]
    public Guid? WaterSpeed { get; set; }

    [GdsFieldErrorClass(GdsFieldTypes.Radio)]
    public Guid? Appearance { get; set; }

    [GdsFieldErrorClass(GdsFieldTypes.Textarea)]
    public string? MoreDetails { get; set; }
}

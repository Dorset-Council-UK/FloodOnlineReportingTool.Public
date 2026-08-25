using GdsBlazorComponents;

namespace FloodOnlineReportingTool.Public.Models.FloodReport.Investigation;

public class Entry
{
    [GdsFieldErrorClass(GdsFieldTypes.Checkbox)]
    public IList<Guid> EntryOptions { get; set; } = [];

    [GdsFieldErrorClass(GdsFieldTypes.Textarea)]
    public string? WaterEnteredOther { get; set; }
}

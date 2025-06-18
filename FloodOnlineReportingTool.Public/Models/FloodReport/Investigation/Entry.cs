using FloodOnlineReportingTool.GdsComponents;

namespace FloodOnlineReportingTool.Public.Models.FloodReport.Investigation;

public class Entry
{
    [GdsFieldErrorClass(GdsFieldTypes.Checkbox)]
    public IReadOnlyCollection<GdsOptionItem<Guid>> EntryOptions { get; set; } = [];

    [GdsFieldErrorClass(GdsFieldTypes.Textarea)]
    public string? WaterEnteredOther { get; set; }
}

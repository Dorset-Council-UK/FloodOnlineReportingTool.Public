using FloodOnlineReportingTool.GdsComponents;

namespace FloodOnlineReportingTool.Public.Models.FloodReport.Investigation;

public class ActionsTaken
{
    [GdsFieldErrorClass(GdsFieldTypes.Checkbox)]
    public IReadOnlyCollection<GdsOptionItem<Guid>> ActionsTakenOptions { get; set; } = [];

    [GdsFieldErrorClass(GdsFieldTypes.Textarea)]
    public string? OtherAction { get; set; }
}

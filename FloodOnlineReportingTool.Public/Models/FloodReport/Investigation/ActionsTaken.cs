using GdsBlazorComponents;

namespace FloodOnlineReportingTool.Public.Models.FloodReport.Investigation;

public class ActionsTaken
{
    [GdsFieldErrorClass(GdsFieldTypes.Checkbox)]
    public IList<Guid> ActionsTakenOptions { get; set; } = [];

    [GdsFieldErrorClass(GdsFieldTypes.Textarea)]
    public string? OtherAction { get; set; }
}

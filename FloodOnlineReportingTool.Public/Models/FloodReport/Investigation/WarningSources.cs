using GdsBlazorComponents;

namespace FloodOnlineReportingTool.Public.Models.FloodReport.Investigation;

public class WarningSources
{
    [GdsFieldErrorClass(GdsFieldTypes.Checkbox)]
    public IList<Guid> WarningSourceOptions { get; set; } = [];

    [GdsFieldErrorClass(GdsFieldTypes.Textarea)]
    public string? WarningOther { get; set; }
}

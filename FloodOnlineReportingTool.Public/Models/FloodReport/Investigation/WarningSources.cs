using FloodOnlineReportingTool.GdsComponents;

namespace FloodOnlineReportingTool.Public.Models.FloodReport.Investigation;

public class WarningSources
{
    [GdsFieldErrorClass(GdsFieldTypes.Checkbox)]
    public IReadOnlyCollection<GdsOptionItem<Guid>> WarningSourceOptions { get; set; } = [];

    [GdsFieldErrorClass(GdsFieldTypes.Textarea)]
    public string? WarningOther { get; set; }
}

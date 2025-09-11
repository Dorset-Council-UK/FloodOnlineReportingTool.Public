using GdsBlazorComponents;

namespace FloodOnlineReportingTool.Public.Models.FloodReport.Create;

public class FloodSecondarySource
{
    [GdsFieldErrorClass(GdsFieldTypes.Checkbox)]
    public IReadOnlyCollection<GdsOptionItem<Guid>> FloodSecondarySourceOptions { get; set; } = [];
}

using GdsBlazorComponents;

namespace FloodOnlineReportingTool.Public.Models.FloodReport.Create;

public class FloodSource
{
    [GdsFieldErrorClass(GdsFieldTypes.Checkbox)]
    public IReadOnlyCollection<GdsOptionItem<Guid>> FloodSourceOptions { get; set; } = [];
}

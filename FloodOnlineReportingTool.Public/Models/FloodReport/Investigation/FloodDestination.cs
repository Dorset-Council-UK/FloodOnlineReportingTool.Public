using GdsBlazorComponents;

namespace FloodOnlineReportingTool.Public.Models.FloodReport.Investigation;

public class FloodDestination
{
    [GdsFieldErrorClass(GdsFieldTypes.Checkbox)]
    public IReadOnlyCollection<GdsOptionItem<Guid>> DestinationOptions { get; set; } = [];
}

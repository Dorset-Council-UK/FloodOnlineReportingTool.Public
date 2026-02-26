using GdsBlazorComponents;

namespace FloodOnlineReportingTool.Public.Models.FloodReport.Investigation;

public class ServiceImpact
{
    [GdsFieldErrorClass(GdsFieldTypes.Radio)]
    public Guid? WereServicesImpactedId { get; set; }

    [GdsFieldErrorClass(GdsFieldTypes.Checkbox)]
    public IReadOnlyCollection<GdsOptionItem<Guid>> ImpactedServicesOptions { get; set; } = [];
}

using FloodOnlineReportingTool.GdsComponents;

namespace FloodOnlineReportingTool.Public.Models.FloodReport.Investigation;

public class CommunityImpact
{
    [GdsFieldErrorClass(GdsFieldTypes.Checkbox)]
    public IReadOnlyCollection<GdsOptionItem<Guid>> CommunityImpactOptions { get; set; } = [];
}

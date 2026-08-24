using GdsBlazorComponents;

namespace FloodOnlineReportingTool.Public.Models.FloodReport.Investigation;

public class CommunityImpact
{
    [GdsFieldErrorClass(GdsFieldTypes.Checkbox)]
    public IList<Guid> CommunityImpactOptions { get; set; } = [];
}

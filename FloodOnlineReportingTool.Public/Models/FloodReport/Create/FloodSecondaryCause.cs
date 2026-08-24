using GdsBlazorComponents;

namespace FloodOnlineReportingTool.Public.Models.FloodReport.Create;

public class FloodSecondaryCause
{
    [GdsFieldErrorClass(GdsFieldTypes.Checkbox)]
    public IList<Guid> SecondaryCauseOptions { get; set; } = [];
}

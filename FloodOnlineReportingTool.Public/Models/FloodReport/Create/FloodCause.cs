using GdsBlazorComponents;

namespace FloodOnlineReportingTool.Public.Models.FloodReport.Create;

public class FloodCause
{
    [GdsFieldErrorClass(GdsFieldTypes.Checkbox)]
    public IList<Guid> CauseOptions { get; set; } = [];
}

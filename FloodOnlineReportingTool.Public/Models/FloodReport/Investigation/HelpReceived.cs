using GdsBlazorComponents;

namespace FloodOnlineReportingTool.Public.Models.FloodReport.Investigation;

public class HelpReceived
{
    [GdsFieldErrorClass(GdsFieldTypes.Checkbox)]
    public IList<Guid> HelpReceivedOptions { get; set; } = [];
}

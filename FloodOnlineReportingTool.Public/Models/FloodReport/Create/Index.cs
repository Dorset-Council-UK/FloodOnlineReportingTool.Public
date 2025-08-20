using GdsBlazorComponents;

namespace FloodOnlineReportingTool.Public.Models.FloodReport.Create;

public class Index
{

    [GdsFieldErrorClass(GdsFieldTypes.Radio)]
    public bool IsAddress { get; set; }
}

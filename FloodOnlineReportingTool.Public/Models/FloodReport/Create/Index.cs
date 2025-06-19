using GdsBlazorComponents;

namespace FloodOnlineReportingTool.Public.Models.FloodReport.Create;

public class Index
{
    [GdsFieldErrorClass(GdsFieldTypes.Input)]
    public string? Postcode { get; set; }

    [GdsFieldErrorClass(GdsFieldTypes.Radio)]
    public bool? PostcodeKnown { get; set; }
}

using GdsBlazorComponents;

namespace FloodOnlineReportingTool.Public.Models.FloodReport.Create;

public class FloodAreas
{
    public bool ShowResidential { get; set; }

    public bool ShowCommercial { get; set; }

    [GdsFieldErrorClass(GdsFieldTypes.Checkbox)]
    public IList<Guid> Residentials { get; set; } = [];

    [GdsFieldErrorClass(GdsFieldTypes.Checkbox)]
    public IList<Guid> Commercials { get; set; } = [];

    [GdsFieldErrorClass(GdsFieldTypes.Radio)]
    public bool? IsUninhabitable { get; set; }
}

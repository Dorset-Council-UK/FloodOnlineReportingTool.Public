using GdsBlazorComponents;

namespace FloodOnlineReportingTool.Public.Models.FloodReport.Create;

public class FloodAreas
{
    public bool ShowResidential { get; set; }

    public bool ShowCommercial { get; set; }

    public IList<Guid> ResidentialOptions { get; set; } = [];

    public IList<Guid> CommercialOptions { get; set; } = [];

    [GdsFieldErrorClass(GdsFieldTypes.Radio)]
    public bool? IsUninhabitable { get; set; }
}

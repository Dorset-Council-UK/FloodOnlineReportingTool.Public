using FloodOnlineReportingTool.GdsComponents;

namespace FloodOnlineReportingTool.Public.Models.FloodReport.Create;

public class FloodAreas
{
    public bool ShowResidential { get; set; }

    public bool ShowCommercial { get; set; }
    
    [GdsFieldErrorClass(GdsFieldTypes.Checkbox)]
    public IReadOnlyCollection<GdsOptionItem<Guid>> ResidentialOptions { get; set; } = [];

    [GdsFieldErrorClass(GdsFieldTypes.Checkbox)]
    public IReadOnlyCollection<GdsOptionItem<Guid>> CommercialOptions { get; set; } = [];

    [GdsFieldErrorClass(GdsFieldTypes.Radio)]
    public bool? IsUninhabitable { get; set; }
}

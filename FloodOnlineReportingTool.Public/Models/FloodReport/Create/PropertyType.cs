using FloodOnlineReportingTool.Database.Models.Responsibilities;
using GdsBlazorComponents;

namespace FloodOnlineReportingTool.Public.Models.FloodReport.Create;

public class PropertyType
{
    public bool IsAddress { get; set; }

    [GdsFieldErrorClass(GdsFieldTypes.Radio)]
    public Guid? Property { get; set; }

    public IReadOnlyCollection<GdsOptionItem<Guid>> PropertyOptions { get; set; } = [];

    public IReadOnlyCollection<Organisation> ResponsibleOrganisations { get; set; } = [];
}

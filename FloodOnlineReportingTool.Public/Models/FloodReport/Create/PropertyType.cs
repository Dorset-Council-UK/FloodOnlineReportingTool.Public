using FloodOnlineReportingTool.Database.Models;
using GdsBlazorComponents;

namespace FloodOnlineReportingTool.Public.Models.FloodReport.Create;

public class PropertyType
{
    public bool FromLocation { get; set; }

    [GdsFieldErrorClass(GdsFieldTypes.Radio)]
    public Guid? Property { get; set; }

    public IReadOnlyCollection<GdsOptionItem<Guid>> PropertyOptions { get; set; } = [];

    public IReadOnlyCollection<Organisation> ResponsibleOrganisations { get; set; } = [];
}

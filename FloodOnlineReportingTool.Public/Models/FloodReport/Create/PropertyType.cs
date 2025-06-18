using FloodOnlineReportingTool.DataAccess.Models;
using FloodOnlineReportingTool.GdsComponents;

namespace FloodOnlineReportingTool.Public.Models.FloodReport.Create;

public class PropertyType
{
    [GdsFieldErrorClass(GdsFieldTypes.Radio)]
    public Guid? Property { get; set; }

    public IReadOnlyCollection<GdsOptionItem<Guid>> PropertyOptions { get; set; } = [];

    public IReadOnlyCollection<Organisation> ResponsibleOrganisations { get; set; } = [];
}

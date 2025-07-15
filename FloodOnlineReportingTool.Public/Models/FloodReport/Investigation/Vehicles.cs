using GdsBlazorComponents;

namespace FloodOnlineReportingTool.Public.Models.FloodReport.Investigation;

public class Vehicles
{
    [GdsFieldErrorClass(GdsFieldTypes.Radio)]
    public Guid? WereVehiclesDamagedId { get; set; }

    [GdsFieldErrorClass(GdsFieldTypes.Input)]
    public string? NumberOfVehiclesDamagedText { get; set; }
    public int? NumberOfVehiclesDamagedNumber { get; set; } // Intensionally using an int here instead of byte
}

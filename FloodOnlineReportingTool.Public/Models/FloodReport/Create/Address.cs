using GdsBlazorComponents;

namespace FloodOnlineReportingTool.Public.Models.FloodReport.Create;

public class Address
{
    [GdsFieldErrorClass(GdsFieldTypes.Select)]
    public long? UPRN { get; set; }

    public double? Easting { get; set; }

    public double? Northing { get; set; }

    public bool IsAddress { get; set; }

    public string? LocationDesc { get; set; }

    [GdsFieldErrorClass(GdsFieldTypes.Input)]
    public string? Postcode { get; set; }

}

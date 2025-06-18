using FloodOnlineReportingTool.GdsComponents;

namespace FloodOnlineReportingTool.Public.Models.FloodReport.Create;

public class Address
{
    [GdsFieldErrorClass(GdsFieldTypes.Select)]
    public long? UPRN { get; set; }

    [GdsFieldErrorClass(GdsFieldTypes.Input)]
    public string? Postcode { get; set; }

    public IList<GdsOptionItem<long>> AddressOptions { get; set; } = [];
}

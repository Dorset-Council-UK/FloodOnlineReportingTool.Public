using FloodOnlineReportingTool.Database.Models;
using GdsBlazorComponents;

namespace FloodOnlineReportingTool.Public.Models.FloodReport.Contact;

public class ContactModel
{
    [GdsFieldErrorClass(GdsFieldTypes.Radio)]
    public ContactRecordType? ContactType { get; set; }

    [GdsFieldErrorClass(GdsFieldTypes.Input)]
    public string? ContactName { get; set; }

    [GdsFieldErrorClass(GdsFieldTypes.Input)]
    public string? EmailAddress { get; set; }

    [GdsFieldErrorClass(GdsFieldTypes.Input)]
    public string? PhoneNumber { get; set; }

    public Guid? Id { get; set; }
}

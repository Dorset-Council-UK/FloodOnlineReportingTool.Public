using FloodOnlineReportingTool.Contracts.Shared;
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

    public bool IsEmailVerified { get; init; } // Read-only in this view model

    [GdsFieldErrorClass(GdsFieldTypes.Input)]
    public string? PhoneNumber { get; set; }

    [GdsFieldErrorClass(GdsFieldTypes.Radio)]
    public bool PrimaryContactRecord { get; set; }

    public Guid? Id { get; set; }

    public Guid? ContactUserId { get; set; }


}

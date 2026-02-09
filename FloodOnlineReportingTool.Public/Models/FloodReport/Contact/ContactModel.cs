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
    public bool IsRecordOwner { get; set; } = false;

    public bool IsEmailVerified { get; set; } = false;

    public bool IsSubscribed { get; init; } // Read-only in this view model

    [GdsFieldErrorClass(GdsFieldTypes.Input)]
    public string? PhoneNumber { get; set; } 

    public Guid? Id { get; set; }

    public string? ContactUserId { get; set; }


}

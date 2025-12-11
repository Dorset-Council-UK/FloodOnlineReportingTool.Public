using FloodOnlineReportingTool.Contracts.Shared;

namespace FloodOnlineReportingTool.Public.Models.FloodReport.Contact;

public class ContactModel
{
    
    public ContactRecordType? ContactType { get; set; } 

    public string? ContactName { get; set; } 

    public string? EmailAddress { get; set; } 
    public bool IsRecordOwner { get; set; } = false;

    public bool IsEmailVerified { get; init; } // Read-only in this view model

    public bool IsSubscribed { get; init; } // Read-only in this view model

    public string? PhoneNumber { get; set; } 

    public Guid? Id { get; set; }

    public Guid? ContactUserId { get; set; }


}

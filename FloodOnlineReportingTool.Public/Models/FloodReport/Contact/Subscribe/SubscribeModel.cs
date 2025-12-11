using FloodOnlineReportingTool.Contracts.Shared;
using FloodOnlineReportingTool.Database.Models.Contact;
using GdsBlazorComponents;

namespace FloodOnlineReportingTool.Public.Models.FloodReport.Contact.Subscribe;

public class SubscribeModel
{
    [GdsFieldErrorClass(GdsFieldTypes.Input)]
    public string ContactName { get; set; } = "";
    [GdsFieldErrorClass(GdsFieldTypes.Input)]
    public string EmailAddress { get; set; } = "";
    [GdsFieldErrorClass(GdsFieldTypes.Input)]
    public string? PhoneNumber { get; set; }
    [GdsFieldErrorClass(GdsFieldTypes.Radio)]
    public ContactRecordType ContactType { get; set; } = ContactRecordType.Unknown;
    public bool IsEmailVerified { get; set; } = false;
    public bool IsSubscribed { get; set; } = false;
    public DateTimeOffset CreatedUtc { get; init; }
    public DateTimeOffset RedactionDate { get; init; }

    public Guid? ContactRecordId { get; set; }
    public ContactRecord? ContactRecord { get; set; }

    public string? ErrorMessage { get; set; }
}

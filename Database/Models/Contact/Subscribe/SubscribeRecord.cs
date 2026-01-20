using FloodOnlineReportingTool.Contracts.Shared;
using FloodOnlineReportingTool.Database.Compliance;
using Microsoft.AspNetCore.Identity;

namespace FloodOnlineReportingTool.Database.Models.Contact.Subscribe;

public record SubscribeRecord
{
    // Whilst this is personal data due being able to identify an individual,
    // we do not mark it as redaction would break foreign key relationships.
    // Field excluded from redaction policy as no personal data will remain after
    // other fields are redacted.
    public Guid Id { get; init; } = Guid.CreateVersion7();

    public bool IsRecordOwner { get; set; } = false;
    public ContactRecordType ContactType { get; set; } = ContactRecordType.Unknown;
    [PersonalData]
    [PiiRedaction]
    public string ContactName { get; set; } = "";
    [PersonalData]
    [PiiRedaction]
    public string EmailAddress { get; set; } = "";
    public bool IsEmailVerified { get; set; } = false;
    [PersonalData]
    [Pii]
    public string? PhoneNumber { get; set; }
    public bool IsSubscribed { get; set; } = false;
    public DateTimeOffset CreatedUtc { get; init; }
    public DateTimeOffset RedactionDate { get; init; }
    [PersonalData]
    [Pii]
    public int? VerificationCode { get; set; }
    public DateTimeOffset? VerificationExpiryUtc { get; set; }

    // Optional foreign key to ContactRecord
    public Guid? ContactRecordId { get; set; }
    public ContactRecord? ContactRecord { get; set; }

}

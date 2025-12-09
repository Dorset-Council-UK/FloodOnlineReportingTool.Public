using FloodOnlineReportingTool.Contracts.Shared;
using FloodOnlineReportingTool.Database.Models.Contact.Subscribe;

namespace FloodOnlineReportingTool.Database.Models.Contact;

/// <summary>
///  A data transfer object representing a contact record. Only the data which can be changed.
/// </summary>
public record ContactRecordDto
{
    public Guid? UserId { get; init; }
    public ContactRecordType ContactType { get; init; } = ContactRecordType.Unknown;
    public SubscribeRecord SubscribeRecord { get; init; } = null!;
    public string ContactName { get; init; } = "";
    public string EmailAddress { get; init; } = "";
    public bool IsEmailVerified { get; init; }
    public string? PhoneNumber { get; init; }
}

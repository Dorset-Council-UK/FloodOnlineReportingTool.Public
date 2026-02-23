using FloodOnlineReportingTool.Contracts.Shared;

namespace FloodOnlineReportingTool.Database.Models.Contact;

/// <summary>
///  A data transfer object representing a contact and subscription record. Only the data which can be changed.
/// </summary>
public record ContactRecordDto
{
    public string? UserId { get; init; }
    public ContactRecordType ContactType { get; init; } = ContactRecordType.Unknown;
    public string ContactName { get; init; } = "";
    public string EmailAddress { get; init; } = "";
    public string? PhoneNumber { get; init; }
    public bool IsRecordOwner { get; init; } = false;
}

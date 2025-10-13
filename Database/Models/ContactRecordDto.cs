namespace FloodOnlineReportingTool.Database.Models;

/// <summary>
///  A data transfer object representing a contact record. Only the data which can be changed.
/// </summary>
public record ContactRecordDto
{
    public Guid? UserId { get; init; }
    public ContactRecordType ContactType { get; init; } = ContactRecordType.Unknown;
    public string ContactName { get; init; } = "";
    public string EmailAddress { get; init; } = "";
    public string? PhoneNumber { get; init; }
}

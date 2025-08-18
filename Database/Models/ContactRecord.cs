namespace FloodOnlineReportingTool.Database.Models;

/// <summary>
///     <para>Represents contact information for individuals reporting flood incidents and seeking assistance.</para>
///     <para>This can represent different types of contact records, for example temporary contact details.</para>
/// </summary>
/// <remarks>
///     <para>Do we need fields to handle Oauth2 logins via B2C or do we just handle it with roles?</para>
///     <para>It would be good to know if this record is linked to a user though!</para>
/// </remarks>
public record ContactRecord
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public Guid FloodReportId { get; set; }
    public ContactRecordType ContactType { get; init; } = ContactRecordType.Unknown;
    public DateTimeOffset CreatedUtc { get; init; }
    public DateTimeOffset? UpdatedUtc { get; init; }
    public string ContactName { get; init; } = "";
    public string EmailAddress { get; init; } = "";
    public string? PhoneNumber { get; init; }
    public DateTimeOffset RedactionDate { get; init; }
}

namespace FloodOnlineReportingTool.Database.Models;

/// <summary>
///     <para>Represents contact information for individuals reporting flood incidents and seeking assistance.</para>
///     <para>This can represent different types of contact records, for example temporary contact details.</para>
/// </summary>
/// <remarks>
///     <para>We are setting Oauth to be optional but if the Oid is set then the user details are coming from an account.</para>
/// </remarks>
public record ContactRecord
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public Guid? Oid { get; set; }
    public ICollection<Guid> FloodReportIds { get; set; } = new List<Guid>();
    public ContactRecordType ContactType { get; init; } = ContactRecordType.Unknown;
    public DateTimeOffset CreatedUtc { get; init; }
    public DateTimeOffset? UpdatedUtc { get; init; }
    public string ContactName { get; init; } = "";
    public string EmailAddress { get; init; } = "";
    public string? PhoneNumber { get; init; }
    public DateTimeOffset RedactionDate { get; init; }
}

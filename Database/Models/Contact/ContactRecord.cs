using FloodOnlineReportingTool.Contracts.Shared;
using FloodOnlineReportingTool.Database.Models.Flood;

namespace FloodOnlineReportingTool.Database.Models.Contact;

/// <summary>
///     <para>Represents contact information for individuals reporting flood incidents and seeking assistance.</para>
///     <para>This can represent different types of contact records, for example tenant and home owner contact details.</para>
/// </summary>
/// <remarks>
///     <para>We are setting Oauth to be optional but if the Oid is set then the user details are coming from an account.</para>
/// </remarks>
public record ContactRecord
{
    public Guid Id { get; init; } = Guid.CreateVersion7();

    public ContactRecordType ContactType { get; init; } = ContactRecordType.Unknown;
    public DateTimeOffset CreatedUtc { get; init; }
    public DateTimeOffset? UpdatedUtc { get; init; }
    public string ContactName { get; set; } = "";
    public string EmailAddress { get; set; } = "";
    public bool IsEmailVerified { get; set; } = false;
    public string? PhoneNumber { get; set; }
    public DateTimeOffset RedactionDate { get; init; }

    // If set, this contact maps to a user account — only user-backed contacts may be associated with many reports.
    public Guid? ContactUserId { get; set; }

    // Many-to-many: flood reports can be associated with multiple contact records
    public ICollection<FloodReport> FloodReports { get; set; } = [];
}

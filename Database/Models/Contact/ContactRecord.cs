using FloodOnlineReportingTool.Contracts.Shared;
using FloodOnlineReportingTool.Database.Models.Flood;

namespace FloodOnlineReportingTool.Database.Models.Contact;

/// <summary>
///     <para>Represents contact information for individuals reporting flood incidents and seeking assistance.</para>
///     <para>This can represent different types of contact records, for example tenant and owner contact details.</para>
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

    // Optional FK used when this contact is only associated with a single FloodReport
    public Guid? FloodReportId { get; set; }
    public FloodReport? FloodReport { get; set; }

    // Navigation: reports where this contact is the owner
    public ICollection<FloodReport> OwnedFloodReports { get; set; } = [];

    // Many-to-many: reports where this contact is listed as an extra contact (user-backed contacts will use this)
    public ICollection<FloodReport> FloodReports { get; set; } = [];
}

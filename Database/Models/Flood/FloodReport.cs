using FloodOnlineReportingTool.Database.Models.Contact;
using FloodOnlineReportingTool.Database.Models.Eligibility;
using FloodOnlineReportingTool.Database.Models.Investigate;
using FloodOnlineReportingTool.Database.Models.Status;

namespace FloodOnlineReportingTool.Database.Models.Flood;

/// <summary>
/// Flood report overview.
/// </summary>
public record FloodReport
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public string Reference { get; init; } = "";
    public DateTimeOffset CreatedUtc { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? MarkedForDeletionUtc { get; init; }
    public required Guid StatusId { get; set; }
    public RecordStatus? Status { get; set; }
    public Guid? EligibilityCheckId { get; init; }
    public EligibilityCheck? EligibilityCheck { get; init; }
    public Guid? InvestigationId { get; init; }
    public Investigation? Investigation { get; init; }

    // Owner: single optional ContactRecord (can be null)
    public Guid? ReportOwnerId { get; set; }
    public ContactRecord? ReportOwner { get; set; }

    public DateTimeOffset? ReportOwnerAccessUntil { get; init; } // was AccessToken.ExpirationUtc in a previous version

    // Extra contacts (many-to-many)
    public IList<ContactRecord> ExtraContactRecords { get; set; } = [];

    // Contacts that are linked by a direct FK on ContactRecord (for non-user contacts)
    // Optional navigation to see those single-associated contacts.
    public IList<ContactRecord> SingleAssociatedContacts { get; set; } = [];
}

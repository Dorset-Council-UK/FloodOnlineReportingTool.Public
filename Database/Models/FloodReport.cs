namespace FloodOnlineReportingTool.Database.Models;

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

    // Optional foreign key to Contact record
    public Guid? ReportOwnerId { get; set; }
    public DateTimeOffset? ReportOwnerAccessUntil { get; init; } // was AccessToken.ExpirationUtc in a previous version

    // Navigation properties
    public ContactRecord? ReportOwner { get; set; } 
    public IList<ContactRecord> ExtraContactRecords { get; set; } = []; // This is likely in addition to the report owner record
}

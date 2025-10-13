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
    public required Guid StatusId { get; init; }
    public RecordStatus? Status { get; init; }
    public Guid? EligibilityCheckId { get; init; }
    public EligibilityCheck? EligibilityCheck { get; init; }
    public Guid? InvestigationId { get; init; }
    public Investigation? Investigation { get; init; }

    // Lookup to contact records this is many to many
    public ICollection<FloodReportContact> ContactRecords { get; init; } = [];

    /// <summary>
    /// User ID from Identity. Nullable to allow for anonymous reports
    /// </summary>
    /// <remarks>Don't use email address, use the ID</remarks>
    public Guid? ReportedByUserId { get; init; }
    public ContactRecordType? ReportedByContactType { get; init; }
    public DateTimeOffset? UserAccessUntilUtc { get; init; } // was AccessToken.ExpirationUtc in a previous version
}

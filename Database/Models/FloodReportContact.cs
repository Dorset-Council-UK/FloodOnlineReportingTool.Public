namespace FloodOnlineReportingTool.Database.Models;

/// <summary>
/// Defines a many-to-many relationship between flood reports and contacts
/// </summary>
public record FloodReportContact
{
    public Guid FloodReportId { get; set; }
    public FloodReport FloodReport { get; set; }

    public Guid ContactRecordId { get; set; }
    public ContactRecord ContactRecord { get; set; }

    public ContactRecordType ContactType { get; set; }
}

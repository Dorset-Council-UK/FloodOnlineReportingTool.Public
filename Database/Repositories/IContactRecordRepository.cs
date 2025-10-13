using FloodOnlineReportingTool.Database.Models;

namespace FloodOnlineReportingTool.Database.Repositories;

public interface IContactRecordRepository
{
    /// <summary>
    /// Get the contact record, for the given user, going via the flood report
    /// </summary>
    Task<ContactRecord?> ReportedByUser(Guid userId, Guid id, CancellationToken ct);

    /// <summary>
    /// Get all contact records for the given user, going via the flood report
    /// </summary>
    Task<IReadOnlyCollection<ContactRecord>> AllReportedByUser(Guid userId, CancellationToken ct);

    /// <summary>
    /// Create a contact record for the user, going via the flood report
    /// </summary>
    Task<ContactRecord> CreateForReport(Guid floodReportId, ContactRecordDto dto, CancellationToken ct);

    /// <summary>
    /// Update the contact record, going via the flood report
    /// </summary>
    Task<ContactRecord> UpdateForUser(Guid userId, Guid id, ContactRecordDto dto, CancellationToken ct);

    /// <summary>
    /// Delete the contact record, going via the flood report
    /// </summary>
    Task DeleteForUser(Guid userId, Guid id, ContactRecordType contactType, CancellationToken ct);

    /// <summary>
    /// Count the number of unused contact record types, going via the flood report
    /// </summary>
    Task<int> CountUnusedRecordTypes(Guid userId, CancellationToken ct);

    /// <summary>
    /// Get the unused contact record types, going via the flood report
    /// </summary>
    Task<IList<ContactRecordType>> GetUnusedRecordTypes(Guid userId, CancellationToken ct);
}

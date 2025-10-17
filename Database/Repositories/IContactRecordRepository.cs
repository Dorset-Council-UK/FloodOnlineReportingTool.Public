using FloodOnlineReportingTool.Database.Models;

namespace FloodOnlineReportingTool.Database.Repositories;

public interface IContactRecordRepository
{
    /// <summary>
    /// Gets a contact record by its ID value
    /// </summary>
    /// <returns></returns>
    Task<ContactRecord?> GetContactById(Guid contactRecordId, CancellationToken ct);

    /// <summary>
    /// Get all contact records associated with a flood report
    /// </summary>
    /// <returns></returns>
    Task<IReadOnlyCollection<ContactRecord>> GetContactsByReport(Guid floodReportId, CancellationToken ct);

    /// <summary>
    /// Get the contact record, for the given user, going via the flood report
    /// </summary>
    Task<FloodReport?> ReportedByUser(Guid contactUserId, Guid floodReportId, CancellationToken ct);

    /// <summary>
    /// Get all contact records for the given user, going via the flood report
    /// </summary>
    Task<IReadOnlyCollection<FloodReport>> AllReportedByUser(Guid contactUserId, CancellationToken ct);

    /// <summary>
    /// Create a contact record for the user, going via the flood report
    /// </summary>
    Task<ContactRecord> CreateForReport(Guid floodReportId, ContactRecordDto dto, CancellationToken ct);

    /// <summary>
    /// Update the contact record, going via the flood report
    /// </summary>
    Task<ContactRecord> UpdateForUser(Guid userId, Guid id, ContactRecordDto dto, CancellationToken ct);

    /// <summary>
    /// Delete the contact record by ID
    /// </summary>
    Task DeleteById(Guid contactRecordId, ContactRecordType contactType, CancellationToken ct);

    /// <summary>
    /// Count the number of unused contact record types, going via the flood report
    /// </summary>
    Task<int> CountUnusedRecordTypes(Guid floodReportId, CancellationToken ct);

    /// <summary>
    /// Get the unused contact record types, going via the flood report
    /// </summary>
    Task<IList<ContactRecordType>> GetUnusedRecordTypes(Guid floodReportId, CancellationToken ct);
}

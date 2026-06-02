using FloodOnlineReportingTool.Contracts.Shared;
using FloodOnlineReportingTool.Database.Models.Contact;
using FloodOnlineReportingTool.Database.Models.ResultModels;

namespace FloodOnlineReportingTool.Database.Repositories;

public interface IContactRecordRepository
{
    /// <summary>
    /// Gets a contact record by its ID
    /// </summary>
    Task<ContactRecord?> Get(Guid contactRecordId, CancellationToken cancellationToken);

    /// <summary>
    /// Get a contact record using the users ID
    /// </summary>
    Task<ContactRecord?> Get(string userId, CancellationToken cancellationToken);

    /// <summary>
    /// Get all other contact records associated with a flood report
    /// </summary>
    Task<IReadOnlyCollection<ContactRecord>> GetContactsByReport(Guid floodReportId, CancellationToken ct);

    /// <summary>
    /// Create a contact record. User ID is optional
    /// </summary>
    /// <remarks>This system is fully responsible for all contact communication. No notifications are sent out at this point.</remarks>
    /// <returns>A result pattern with the created contact record, or a list of errors.</returns>
    Task<Result<ContactRecord>> Create(string? userId, CancellationToken cancellationToken);

    /// <summary>
    /// Create a contact record. User ID and flood report ID are optional
    /// </summary>
    /// <remarks>This system is fully responsible for all contact communication. No notifications are sent out at this point.</remarks>
    /// <returns>A result pattern with the created contact record, or a list of errors.</returns>
    Task<Result<ContactRecord>> Create(string? userId, Guid? floodReportId, CancellationToken cancellationToken);

    /// <summary>
    /// Adds the provided flood report to an existing contact record
    /// </summary>
    /// <returns>A result pattern with the updated contact record, or a list of errors.</returns>
    Task<Result<ContactRecord>> LinkContactByReport(Guid floodReportId, Guid contactRecordId, CancellationToken ct);

    /// <summary>
    /// Update the contact record, going via the flood report
    /// </summary>
    /// <remarks>This system is fully responsible for all contact communication. No notifications are sent out at this point.</remarks>
    /// <returns>A result pattern with the updated contact record, or a list of errors.</returns>
    Task<Result<ContactRecord>> UpdateForUser(string userId, Guid contactRecordId, CancellationToken ct);

    /// <summary>
    /// Delete the contact record by ID
    /// </summary>
    /// <remarks>This system is fully responsible for all contact communication. No notifications are sent out at this point.</remarks>
    /// <returns>A result pattern indicating success, or a list of errors.</returns>
    Task<DeleteResult<ContactRecord>> DeleteById(Guid contactRecordId, ContactRecordType contactType, CancellationToken ct);

    /// <summary>
    /// Count the number of contact records
    /// </summary>
    Task<int> Count(CancellationToken cancellationToken);

    /// <summary>
    /// Count the number of contact records for a user
    /// </summary>
    Task<int> Count(string userId, CancellationToken cancellationToken);

    /// <summary>
    /// Count the number of unused contact record types, going via the flood report
    /// </summary>
    Task<int> CountUnusedRecordTypes(Guid floodReportId, CancellationToken ct);

    /// <summary>
    /// Get the unused contact record types, going via the flood report
    /// </summary>
    Task<IList<ContactRecordType>> GetUnusedRecordTypes(Guid floodReportId, CancellationToken ct);

    /// <summary>
    /// Does the contact record exist using the ID
    /// </summary>
    Task<bool> Exists(Guid contactRecordId, CancellationToken cancellationToken);

    /// <summary>
    /// Does the contact record exist for the user
    /// </summary>
    Task<bool> Exists(string userId, CancellationToken cancellationToken);

    /// <summary>
    /// Does the contact record exist for the flood report and contact record type
    /// </summary>
    Task<bool> Exists(Guid floodReportId, ContactRecordType contactRecordType, CancellationToken cancellationToken);
}

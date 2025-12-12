using FloodOnlineReportingTool.Contracts.Shared;
using FloodOnlineReportingTool.Database.Models.Contact;
using FloodOnlineReportingTool.Database.Models.Contact.Subscribe;

namespace FloodOnlineReportingTool.Database.Repositories;

public interface IContactRecordRepository
{
    /// <summary>
    /// Gets a contact record by its ID value
    /// </summary>
    Task<ContactRecord?> GetContactById(Guid contactRecordId, CancellationToken ct);

    /// <summary>
    /// Get the report owner contact records associated with a flood report
    /// </summary>
    /// <returns></returns>
    Task<SubscribeRecord?> GetReportOwnerContactByReport(Guid floodReportId, CancellationToken ct);

    /// <summary>
    /// Get all other contact records associated with a flood report
    /// </summary>
    Task<IReadOnlyCollection<ContactRecord>> GetContactsByReport(Guid floodReportId, CancellationToken ct);

    /// <summary>
    /// Create a contact record for the user, going via the flood report
    /// </summary>
    /// <remarks>This system is fully responsible for all contact communication. No notifications are sent out at this point.</remarks>
    Task<ContactRecordCreateOrUpdateResult> CreateForReport(Guid floodReportId, ContactRecordDto dto, CancellationToken ct);

    /// <summary>
    /// Adds the provided flood report to an existing contact record
    /// </summary>
    /// <returns></returns>
    Task<bool> LinkContactByReport(Guid floodReportId, Guid contactRecordId, CancellationToken ct);
    /// <summary>
    /// Update the contact record, going via the flood report
    /// </summary>
    /// <remarks>This system is fully responsible for all contact communication. No notifications are sent out at this point.</remarks>
    Task<ContactRecordCreateOrUpdateResult> UpdateForUser(Guid userId, Guid contactRecordId, ContactRecordDto dto, CancellationToken ct);

    /// <summary>
    /// Delete the contact record by ID
    /// </summary>
    /// <remarks>This system is fully responsible for all contact communication. No notifications are sent out at this point.</remarks>
    Task<ContactRecordDeleteResult> DeleteById(Guid contactRecordId, ContactRecordType contactType, CancellationToken ct);

    /// <summary>
    /// Creates a contact subscription record
    /// </summary>
    /// <returns>This record will be linked to a contact record once completed. Unlinked records will be deleted after retention date.</returns>
    Task<SubscribeCreateOrUpdateResult> CreateSubscriptionRecord(Guid contactRecordId, ContactRecordDto dto, string? userEmail, CancellationToken ct);

    /// <summary>
    /// Returns a current subscription record by its ID
    /// </summary>
    /// <returns></returns>
    Task<SubscribeRecord?> GetSubscriptionRecordById(Guid subscriptionId, CancellationToken ct);

    /// <summary>
    /// Verifies a contact subscription record
    /// </summary>
    /// <returns></returns>
    Task<bool> VerifySubscriptionRecord(Guid subscriptionId, int verificationCode, CancellationToken ct);

    /// <summary>
    /// This updates the verification code and expiry on a subscription record
    /// </summary>
    /// <returns></returns>
    Task<SubscribeCreateOrUpdateResult> UpdateVerificationCode(SubscribeRecord subscriptionRecord, CancellationToken ct);

    /// <summary>
    /// Updates a subscription record
    /// </summary>
    /// <returns></returns>
    Task<SubscribeCreateOrUpdateResult> UpdateSubscriptionRecord(SubscribeRecord subscriptionRecord, CancellationToken ct);

    /// <summary>
    /// Deletes a subscription record by its ID
    /// </summary>
    /// <returns></returns>
    Task<SubscribeDeleteResult> DeleteSubscriptionById(Guid subscriptionId, CancellationToken ct);

    /// <summary>
    /// Count the number of unused contact record types, going via the flood report
    /// </summary>
    Task<int> CountUnusedRecordTypes(Guid floodReportId, CancellationToken ct);

    /// <summary>
    /// Get the unused contact record types, going via the flood report
    /// </summary>
    Task<IList<ContactRecordType>> GetUnusedRecordTypes(Guid floodReportId, CancellationToken ct);

    Task<bool> ContactRecordExists(Guid contactRecordId, CancellationToken ct = default);

    Task<Guid?> ContactRecordExistsForUser(Guid userId, CancellationToken ct = default);
}

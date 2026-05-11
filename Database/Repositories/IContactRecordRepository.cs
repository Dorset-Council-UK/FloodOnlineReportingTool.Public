using FloodOnlineReportingTool.Contracts.Shared;
using FloodOnlineReportingTool.Database.Models.Contact;
using FloodOnlineReportingTool.Database.Models.Contact.Subscribe;
using FloodOnlineReportingTool.Database.Models.ResultModels;

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
    /// <returns>A result pattern with the created contact record, or a list of errors.</returns>
    Task<Result<ContactRecord>> CreateForReport(Guid floodReportId, ContactRecordDto dto, CancellationToken ct);

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
    Task<Result<ContactRecord>> UpdateForUser(string userId, Guid contactRecordId, ContactRecordDto dto, CancellationToken ct);

    /// <summary>
    /// Delete the contact record by ID
    /// </summary>
    /// <remarks>This system is fully responsible for all contact communication. No notifications are sent out at this point.</remarks>
    /// <returns>A result pattern indicating success, or a list of errors.</returns>
    Task<DeleteResult<ContactRecord>> DeleteById(Guid contactRecordId, ContactRecordType contactType, CancellationToken ct);

    /// <summary>
    /// Creates a contact subscription record
    /// </summary>
    /// <returns>
    ///     <para>A result pattern with the created subscribe record, or a list of errors.</para>
    ///     <para>This record will be linked to a contact record once completed. Unlinked records will be deleted after retention date.</para>
    /// </returns>
    Task<Result<SubscribeRecord>> CreateSubscriptionRecord(Guid contactRecordId, ContactRecordDto dto, string? userEmail, bool userPresent, CancellationToken ct);

    /// <summary>
    /// Returns a current subscription record by its ID
    /// </summary>
    Task<SubscribeRecord?> GetSubscriptionRecordById(Guid subscriptionId, CancellationToken ct);

    /// <summary>
    /// Verifies a contact subscription record
    /// </summary>
    Task<bool> VerifySubscriptionRecord(Guid subscriptionId, int verificationCode, CancellationToken ct);

    /// <summary>
    /// This updates the verification code and expiry on a subscription record
    /// </summary>
    Task<Result<SubscribeRecord>> UpdateVerificationCode(SubscribeRecord subscriptionRecord, bool userPresent, CancellationToken ct);

    /// <summary>
    /// Updates a subscription record
    /// </summary>
    /// <returns>A result pattern with the updated subscribe record, or a list of errors.</returns>
    Task<Result<SubscribeRecord>> UpdateSubscriptionRecord(SubscribeRecord subscriptionRecord, CancellationToken ct);

    /// <summary>
    /// Deletes a subscription record by its ID
    /// </summary>
    /// <returns>A result pattern indicating success, or a list of errors.</returns>
    Task<DeleteResult<SubscribeRecord>> DeleteSubscriptionById(Guid subscriptionId, CancellationToken ct);

    /// <summary>
    /// Count the number of unused contact record types, going via the flood report
    /// </summary>
    Task<int> CountUnusedRecordTypes(Guid floodReportId, CancellationToken ct);

    /// <summary>
    /// Get the unused contact record types, going via the flood report
    /// </summary>
    Task<IList<ContactRecordType>> GetUnusedRecordTypes(Guid floodReportId, CancellationToken ct);

    Task<bool> ContactRecordExists(Guid contactRecordId, CancellationToken ct = default);

    Task<Guid?> ContactRecordExistsForUser(string userId, CancellationToken ct = default);

    Task<Guid> GetRandomFloodReportWithSubscriber(CancellationToken ct = default);
}

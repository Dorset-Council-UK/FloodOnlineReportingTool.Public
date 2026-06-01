using FloodOnlineReportingTool.Database.Models.Contact.Subscribe;
using FloodOnlineReportingTool.Database.Models.ResultModels;

namespace FloodOnlineReportingTool.Database.Repositories;

public interface ISubscribeRecordRepository
{
    /// <summary>
    /// Count the number of subscribe records
    /// </summary>
    Task<int> Count(CancellationToken cancellationToken);

    /// <summary>
    /// Count the number of subscribe records for a contact record
    /// </summary>
    Task<int> Count(Guid contactRecordId, CancellationToken cancellationToken);

    /// <summary>
    /// Count the number of subscribe records for the user ID
    /// </summary>
    Task<int> Count(string userId, CancellationToken cancellationToken);

    /// <summary>
    /// Does the subscribe record exist using the ID
    /// </summary>
    Task<bool> Exists(Guid subscribeRecordId, CancellationToken cancellationToken);

    /// <summary>
    /// Get all the subscription records for a contact record
    /// </summary>
    Task<IReadOnlyCollection<SubscribeRecord>> GetAll(Guid contactRecordId, CancellationToken cancellationToken);

    /// <summary>
    /// Get all the subscription records
    /// </summary>
    Task<IReadOnlyCollection<SubscribeRecord>> GetAll(CancellationToken cancellationToken);

    /// <summary>
    /// Get a subscription record by subscription ID
    /// </summary>
    Task<SubscribeRecord?> Get(Guid subscriptionId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a subscribe record by the contact record ID and subscribe record ID
    /// </summary>
    Task<SubscribeRecord?> Get(Guid contactRecordId, Guid subscribeRecordId, CancellationToken cancellationToken);

    /// <summary>
    /// Get the report owner contact records associated with a flood report
    /// </summary>
    /// <returns></returns>
    Task<SubscribeRecord?> GetReportOwnerContactByReport(Guid floodReportId, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a contact subscription record
    /// </summary>
    /// <returns>
    ///     <para>A result pattern with the created subscribe record, or a list of errors.</para>
    ///     <para>This record will be linked to a contact record once completed. Unlinked records will be deleted after retention date.</para>
    /// </returns>
    Task<Result<SubscribeRecord>> Create(Guid contactRecordId, SubscribeRecordDto subscribeRecordDto, string? userEmail, bool userPresent, CancellationToken cancellationToken);

    /// <summary>
    /// Updates a subscription record
    /// </summary>
    /// <returns>A result pattern with the updated subscribe record, or a list of errors.</returns>
    Task<Result<SubscribeRecord>> Update(SubscribeRecord subscriptionRecord, CancellationToken cancellationToken);

    /// <summary>
    /// Updates a subscription record
    /// </summary>
    /// <returns>A result pattern with the updated subscribe record, or a list of errors.</returns>
    Task<Result<SubscribeRecord>> Update(Guid subscribeRecordId, SubscribeRecordDto subscribeRecordDto, CancellationToken cancellationToken);

    /// <summary>
    /// This updates the verification code and expiry on a subscription record
    /// </summary>
    /// <returns>A result pattern indicating success, or a list of errors.</returns>
    Task<Result<SubscribeRecord>> UpdateVerificationCode(SubscribeRecord subscriptionRecord, bool userPresent, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a subscription record by its ID
    /// </summary>
    /// <returns>A result pattern indicating success, or a list of errors.</returns>
    Task<DeleteResult<SubscribeRecord>> Delete(Guid subscribeRecordId, CancellationToken cancellationToken);

    /// <summary>
    /// Verifies a contact subscription record
    /// </summary>
    Task<bool> Verify(Guid subscribeRecordId, int verificationCode, CancellationToken cancellationToken);
}

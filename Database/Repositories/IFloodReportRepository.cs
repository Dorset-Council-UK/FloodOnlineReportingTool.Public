using FloodOnlineReportingTool.Database.Models.Eligibility;
using FloodOnlineReportingTool.Database.Models.Flood;

namespace FloodOnlineReportingTool.Database.Repositories;

public interface IFloodReportRepository
{
    /// <summary>
    /// Get a flood report, for the given user
    /// </summary>
    Task<FloodReport?> ReportedByUser(Guid userId, CancellationToken ct);

    /// <summary>
    /// Get the contact record, for the given user, going via the flood report
    /// </summary>
    Task<FloodReport?> ReportedByContact(Guid contactUserId, Guid floodReportId, CancellationToken ct);

    /// <summary>
    /// Get all contact records for the given user, going via the flood report
    /// </summary>
    Task<IReadOnlyCollection<FloodReport>> AllReportedByContact(Guid contactUserId, CancellationToken ct);

    /// <summary>
    /// This enables contact subscriptions for the flood report
    /// </summary>
    /// <returns></returns>
    Task<FloodReportCreateOrUpdateResult> EnableContactSubscriptionsForReport(Guid floodReportId, CancellationToken ct);

    /// <summary>
    /// Flood report by ID.
    /// </summary>
    /// <returns></returns>
    Task<FloodReport?> GetById(Guid reference, CancellationToken ct);

    /// <summary>
    /// Flood report reference number, with no dashes.
    /// </summary>
    Task<FloodReport?> GetByReference(string reference, CancellationToken ct);

    /// <summary>
    /// Get basic flood report information for the given user
    /// </summary>
    Task<(bool hasFloodReport, bool hasInvestigation, bool hasInvestigationStarted, DateTimeOffset? investigationCreatedUtc)> ReportedByUserBasicInformation(Guid userId, CancellationToken ct);

    /// <summary>
    ///     <para>Create a new flood report.</para>
    ///     <para>Publish a message to the message system.</para>
    /// </summary>
    Task<FloodReport> Create(CancellationToken ct);

    /// <summary>
    ///     <para>Create a new flood report, with eligitlity check.</para>
    ///     <para>Publish a message to the message system.</para>
    /// </summary>
    Task<FloodReport> CreateWithEligiblityCheck(EligibilityCheckDto dto, CancellationToken ct);

    /// <summary>
    ///     <para>Gets the result model with information about the eligibility status of the current record</para>
    /// </summary>
    Task<EligibilityResult> CalculateEligibilityWithReference(string reference, CancellationToken ct);
    bool HasInvestigationStarted(Guid status);
}

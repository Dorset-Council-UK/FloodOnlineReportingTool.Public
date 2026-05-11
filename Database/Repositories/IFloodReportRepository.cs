using FloodOnlineReportingTool.Database.Models.Eligibility;
using FloodOnlineReportingTool.Database.Models.Flood;
using FloodOnlineReportingTool.Database.Models.ResultModels;

namespace FloodOnlineReportingTool.Database.Repositories;

public interface IFloodReportRepository
{
    /// <summary>
    /// Get all flood reports, for the given user
    /// </summary>
    Task<IReadOnlyCollection<FloodReport>> ReportedByUser(string userId, CancellationToken ct);

    /// <summary>
    /// Get a flood report, for the given user
    /// </summary>
    Task<FloodReport?> ReportedByUser(string userId, Guid floodReportId, CancellationToken ct);

    /// <summary>
    /// Get a flood report, for the given user
    /// </summary>
    Task<FloodReport?> ReportedByUser(string userId, string reference, CancellationToken ct);

    /// <summary>
    /// This enables contact subscriptions for the flood report
    /// </summary>
    /// <returns>A result pattern with the updated flood report contact records, or a list of errors.</returns>
    Task<Result<FloodReport>> EnableContactSubscriptionsForReport(Guid floodReportId, CancellationToken ct);

    /// <summary>
    /// Get all flood reports, with simple overview information.
    /// </summary>
    /// <remarks>Includes Status and EligibilityCheck. Not Investigation or ContactRecords. No related entities.</remarks>
    Task<IReadOnlyCollection<FloodReport>> GetAllOverview(CancellationToken ct);

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
    /// Check if a flood report exists with the given reference number.
    /// </summary>
    Task<bool> ReportWithReferenceExists(string reference, CancellationToken ct);

    /// <summary>
    /// Get basic flood report information for the given user
    /// </summary>
    Task<(bool hasFloodReport, bool hasInvestigation, bool hasInvestigationStarted, DateTimeOffset? investigationCreatedUtc)> InvestigationBasicInformation(Guid FloodReportId, CancellationToken ct);

    /// <summary>
    ///     <para>Create a new flood report, with eligitlity check.</para>
    ///     <para>Publishes a message to the message system.</para>
    /// </summary>
    /// <returns>A result pattern with the created flood report, or a list of errors.</returns>
    Task<Result<FloodReport>> Create(EligibilityCheckDto dto, Uri viewUriBase, CancellationToken ct); // CreateWithEligiblityCheck

    /// <summary>
    ///     <para>Updates a flood report with new eligibility check information.</para>
    ///     <para>Publishes a message to the message system.</para>
    /// </summary>
    /// <returns>A result pattern with the updated flood report, or a list of errors.</returns>
    Task<Result<FloodReport?>> Update(Guid id, EligibilityCheckDto dto, Guid status, Uri viewUriBase, CancellationToken ct);

    /// <summary>
    ///     <para>Updates a users flood report with new eligibility check information.</para>
    ///     <para>Publishes a message to the message system.</para>
    /// </summary>
    /// <returns>A result pattern with the users updated flood report, or a list of errors.</returns>
    Task<Result<FloodReport?>> Update(string userId, Guid id, EligibilityCheckDto dto, Guid status, Uri viewUriBase, CancellationToken ct);

    /// <summary>
    ///     <para>Gets the result model with information about the eligibility status of the current record</para>
    /// </summary>
    Task<EligibilityResult> CalculateEligibilityWithReference(string reference, CancellationToken ct);
    bool HasInvestigationStarted(Guid status);
}

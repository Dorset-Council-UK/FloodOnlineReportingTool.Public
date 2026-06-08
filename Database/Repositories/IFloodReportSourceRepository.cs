using FloodOnlineReportingTool.Database.Models.Eligibility;
using FloodOnlineReportingTool.Database.Models.Flood;
using FloodOnlineReportingTool.Database.Models.ResultModels;

namespace FloodOnlineReportingTool.Database.Repositories;

public interface IFloodReportSourceRepository
{
    /// <summary>
    /// Count the number of flood report sources
    /// </summary>
    Task<int> Count(CancellationToken cancellationToken);

    /// <summary>
    /// Count the number of flood report sources the user has
    /// </summary>
    Task<int> Count(string userId, CancellationToken cancellationToken);

    /// <summary>
    /// Get all flood report sources, for the given user
    /// </summary>
    Task<IReadOnlyCollection<FloodReportSource>> ReportedByUser(string userId, CancellationToken ct);

    /// <summary>
    /// Get a flood report source, for the given user
    /// </summary>
    Task<FloodReportSource?> ReportedByUser(string userId, Guid floodReportSourceId, CancellationToken ct);

    /// <summary>
    /// Get a flood report source, for the given user
    /// </summary>
    Task<FloodReportSource?> ReportedByUser(string userId, string reference, CancellationToken ct);

    /// <summary>
    /// This enables contact subscriptions for the flood report source
    /// </summary>
    /// <returns>A result pattern with the updated flood report source contact records, or a list of errors.</returns>
    Task<Result<FloodReportSource>> EnableContactSubscriptionsForReport(Guid floodReportSourceId, CancellationToken ct);

    /// <summary>
    /// Get all flood report sources, with simple overview information.
    /// </summary>
    /// <remarks>Includes Status and EligibilityCheck. Not Investigation or ContactRecords. No related entities.</remarks>
    Task<IReadOnlyCollection<FloodReportSource>> GetAllOverview(CancellationToken ct);

    /// <summary>
    /// Flood report source by ID.
    /// </summary>
    Task<FloodReportSource?> GetById(Guid floodReportSourceId, CancellationToken ct);

    /// <summary>
    /// Flood report source reference number, with no dashes.
    /// </summary>
    Task<FloodReportSource?> GetByReference(string reference, CancellationToken ct);

    /// <summary>
    /// Check if a flood report source exists with the given reference number.
    /// </summary>
    Task<bool> ReportWithReferenceExists(string reference, CancellationToken ct);

    /// <summary>
    /// Get basic flood report source information for the given user
    /// </summary>
    Task<(bool hasFloodReportSource, bool hasInvestigation, bool hasInvestigationStarted, DateTimeOffset? investigationCreatedUtc)> InvestigationBasicInformation(Guid floodReportSourceId, CancellationToken ct);

    /// <summary>
    ///     <para>Create a new flood report source, with eligitlity check.</para>
    ///     <para>Publishes a message to the message system.</para>
    /// </summary>
    /// <returns>A result pattern with the created flood report source, or a list of errors.</returns>
    Task<Result<FloodReportSource>> Create(EligibilityCheckDto dto, Uri viewUriBase, CancellationToken ct); // CreateWithEligiblityCheck

    /// <summary>
    ///     <para>Updates a flood report source with new eligibility check information.</para>
    ///     <para>Publishes a message to the message system.</para>
    /// </summary>
    /// <returns>A result pattern with the updated flood report source, or a list of errors.</returns>
    Task<Result<FloodReportSource?>> Update(Guid eligibilityCheckId, EligibilityCheckDto dto, Guid status, Uri viewUriBase, CancellationToken ct);

    /// <summary>
    ///     <para>Updates a users flood report source with new eligibility check information.</para>
    ///     <para>Publishes a message to the message system.</para>
    /// </summary>
    /// <returns>A result pattern with the users updated flood report source, or a list of errors.</returns>
    Task<Result<FloodReportSource?>> Update(string userId, Guid eligibilityCheckId, EligibilityCheckDto dto, Guid status, Uri viewUriBase, CancellationToken ct);

    /// <summary>
    ///     <para>Gets the result model with information about the eligibility status of the current record</para>
    /// </summary>
    Task<EligibilityResult> CalculateEligibilityWithReference(string reference, CancellationToken ct);
    bool HasInvestigationStarted(Guid status);
}

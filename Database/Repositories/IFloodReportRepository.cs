using FloodOnlineReportingTool.Database.Models;

namespace FloodOnlineReportingTool.Database.Repositories;

public interface IFloodReportRepository
{
    /// <summary>
    /// Get a flood report, for the given user
    /// </summary>
    Task<FloodReport?> ReportedByUser(Guid userId, CancellationToken ct);

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

    bool HasInvestigationStarted(Guid status);
}

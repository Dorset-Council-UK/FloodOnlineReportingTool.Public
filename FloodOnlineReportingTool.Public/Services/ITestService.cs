using FloodOnlineReportingTool.Database.Models.Flood;
using FloodOnlineReportingTool.Database.Models.Investigate;
using FloodOnlineReportingTool.Database.Models.Messaging;

namespace FloodOnlineReportingTool.Public.Services;

public interface ITestService
{
    /// <summary>
    /// Create a test flood report with a test eligibility check
    /// </summary>
    /// <remarks>This only runs in development!</remarks>
    Task<FloodReport?> TestFloodReport_Create(CancellationToken cancellationToken);

    Task<Investigation?> TestInvestigation(CancellationToken cancellationToken);

    Task<InvestigationDto?> TestInvestigationDto(CancellationToken cancellationToken);


    Task TestFloodReportActionNeededStatus(Guid floodReportId, CancellationToken cancellationToken);

    /// <summary>
    /// Create a test outbox FloodReportSourceCreated message with the given message status
    /// </summary>
    /// <remarks>This only runs in development!</remarks>
    Task<OutboxMessage?> TestOutboxMessage_FloodReportSourceCreated(MessageStatus messageStatus, CancellationToken cancellationToken);

    /// <summary>
    /// Create a test outbox FloodReportSourceUpdated message with the given message status
    /// </summary>
    /// <remarks>This only runs in development!</remarks>
    Task<OutboxMessage?> TestOutboxMessage_FloodReportSourceUpdated(MessageStatus messageStatus, CancellationToken cancellationToken);
}

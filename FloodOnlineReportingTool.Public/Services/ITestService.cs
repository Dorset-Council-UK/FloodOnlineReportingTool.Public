using FloodOnlineReportingTool.Database.Models.Contact.Subscribe;
using FloodOnlineReportingTool.Database.Models.Eligibility;
using FloodOnlineReportingTool.Database.Models.Investigate;
using FloodOnlineReportingTool.Database.Models.Messaging;
using FloodOnlineReportingTool.Public.Models.FloodReport.Create;

namespace FloodOnlineReportingTool.Public.Services;

public interface ITestService
{
    EligibilityCheckDto TestData_EligibilityCheckDto { get; }
    ExtraData TestData_EligibilityCheck_ExtraData { get; }
    InvestigationDto TestData_InvestigationDto { get; }
    SubscribeRecordDto TestData_SubscribeRecordDto { get; }

    Task TestFloodReport_SetInvestigationHasStarted(Guid floodReportId, CancellationToken cancellationToken);

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

    /// <summary>
    /// This function is for use in integration tests only.
    /// </summary>
    Task<Guid?> GetRandomFloodReportWithSubscriber(CancellationToken cancellationToken);
}

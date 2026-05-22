using FloodOnlineReportingTool.Database.Models.Contact;
using FloodOnlineReportingTool.Database.Models.Eligibility;
using FloodOnlineReportingTool.Database.Models.Flood;
using FloodOnlineReportingTool.Database.Models.Investigate;
using FloodOnlineReportingTool.Database.Models.Messaging;
using FloodOnlineReportingTool.Public.Models.FloodReport.Create;

namespace FloodOnlineReportingTool.Public.Services;

public interface ITestService
{
    EligibilityCheckDto TestData_EligibilityCheckDto { get; }
    ExtraData TestData_EligibilityCheck_ExtraData { get; }
    InvestigationDto TestData_InvestigationDto { get; }

    /// <summary>
    /// Create a test contact record assosicated with a flood report
    /// </summary>
    /// <remarks>This only runs in development!</remarks>
    Task<ContactRecord?> TestContactRecord_Create(Guid floodReportId, string userId, CancellationToken cancellationToken);

    /// <summary>
    /// Create a test flood report with a test eligibility check
    /// </summary>
    /// <remarks>This only runs in development!</remarks>
    Task<FloodReport?> TestFloodReport_Create(CancellationToken cancellationToken);

    /// <summary>
    /// Get your last flood report
    /// </summary>
    /// <remarks>This only runs in development!</remarks>
    Task<FloodReport?> TestFloodReport_GetLast(string userId, CancellationToken cancellationToken);

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

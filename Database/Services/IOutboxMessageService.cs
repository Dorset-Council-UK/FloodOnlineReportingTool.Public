using FloodOnlineReportingTool.Database.Models.Messaging;

namespace FloodOnlineReportingTool.Database.Services;

public interface IOutboxMessageService
{
    /// <summary>
    /// Get all outbox messages
    /// </summary>
    Task<IReadOnlyCollection<OutboxMessage>> Get(CancellationToken cancellationToken);

    /// <summary>
    /// Get all outbox messages with the given status
    /// </summary>
    Task<IReadOnlyCollection<OutboxMessage>> Get(MessageStatus messageStatus, CancellationToken cancellationToken);

    /// <summary>
    /// Get all outbox messages with any of the given statuses
    /// </summary>
    Task<IReadOnlyCollection<OutboxMessage>> Get(MessageStatus[] messageStatuses, CancellationToken cancellationToken);

    /// <summary>
    /// Get an outbox message by ID
    /// </summary>
    Task<OutboxMessage?> Get(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Count the number of outbox messages
    /// </summary>
    Task<int> Count(CancellationToken cancellationToken);

    /// <summary>
    /// Count the number of outbox messages with the given status
    /// </summary>
    Task<int> Count(MessageStatus messageStatus, CancellationToken cancellationToken);

    /// <summary>
    /// Count the number of outbox messages with any of the given statuses
    /// </summary>
    Task<int> Count(MessageStatus[] messageStatuses, CancellationToken cancellationToken);
}

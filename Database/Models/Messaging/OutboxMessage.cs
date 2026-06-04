namespace FloodOnlineReportingTool.Database.Models.Messaging;

public record OutboxMessage
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public DateTimeOffset Created { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? Delivered { get; set; }
    public MessageStatus Status { get; set; } = MessageStatus.Unknown;
    public string? ErrorReason { get; set; }
    public MessagePriority Priority { get; init; } = MessagePriority.Medium;
    public required string MessageType { get; init; }
    public required string Message { get; init; }
}

using Azure.Messaging.ServiceBus;
using FloodOnlineReportingTool.Contracts;
using FloodOnlineReportingTool.Contracts.Topics;
using FloodOnlineReportingTool.Database.DbContexts;
using FloodOnlineReportingTool.Database.Models.Messaging;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Globalization;
using System.Net.Mime;

namespace Outbox;

public sealed class Worker(
    ILogger<Worker> logger,
    IServiceProvider serviceProvider,
    ServiceBusClient serviceBusClient
) : BackgroundService
{
    public const string ActivitySourceName = "OutboxWorker";
    private static readonly ActivitySource s_activitySource = new(ActivitySourceName);
    private const int BatchSize = 10;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            using var activity = s_activitySource.StartActivity("Processing outbox messages", ActivityKind.Producer);

            try
            {
                // get the pending outbox messages
                await using var scope = serviceProvider.CreateAsyncScope();
                var publicDbContext = scope.ServiceProvider.GetRequiredService<PublicDbContext>();
                List<OutboxMessage> pendingMessages = await publicDbContext.OutboxMessages
                    .Where(m => m.Status == MessageStatus.Pending)
                    .OrderBy(m => m.Priority)
                    .ThenBy(m => m.Created) // FIFO processing
                    .Take(BatchSize)
                    .ToListAsync(cancellationToken);
                // TODO: add priority to OutboxMessage for stronger grouping?

                if (groupedPendingMessages.Count > 0)
                {

                    foreach (var group in groupedPendingMessages)
                    {
                        logger.LogDebug("Processing {MessageCount} outbox message(s) of type {MessageType}", group.Count(), group.Key);

                        string topicName = GetTopicName(group.Key);
                        await using ServiceBusSender sender = serviceBusClient.CreateSender(topicName);
                        using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync(cancellationToken);

                        foreach (var outboxMessage in group)
                        {
                            ServiceBusMessage message = new(outboxMessage.Message)
                            {
                                ContentType = MediaTypeNames.Application.Json,
                                MessageId = outboxMessage.Id.ToString(),
                                Subject = outboxMessage.MessageType,
                                ApplicationProperties =
                                {
                                    { "Priority", outboxMessage.Priority },
                                },
                            };

                            if (messageBatch.TryAddMessage(message))
                            {
                                outboxMessage.Status = MessageStatus.Processed;
                                outboxMessage.Delivered = DateTimeOffset.UtcNow;
                            }
                            else
                            {
                                // TODO: Build an admin view to see failed messages
                                outboxMessage.Status = MessageStatus.Failed;
                                outboxMessage.ErrorReason = string.Create(CultureInfo.InvariantCulture, $"Message size {message.Body.ToMemory().Length} bytes exceeds the maximum batch size of {messageBatch.MaxSizeInBytes} bytes.");
                                logger.LogError("Outbox message {MessageId} is too large to fit in the batch and will be marked as failed", outboxMessage.Id);
                            }
                        }

                        //await sender.SendMessagesAsync(messageBatch, cancellationToken);
                        //await publicDbContext.SaveChangesAsync(cancellationToken);

                        logger.LogDebug("Sent {SentCount} outbox message(s) to topic {TopicName} ({FailedCount} failed)", messageBatch.Count, topicName, group.Count() - messageBatch.Count);
                    }
                }
                else
                {
                    logger.LogDebug("No pending outbox messages");
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                activity?.AddException(ex);
                logger.LogError(ex, "Error processing outbox messages");
            }

            await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
        }
    }

    /// <summary>
    /// Retrieves the topic name associated with the specified message type.
    /// </summary>
    /// <param name="messageType">The name of the message type for which to obtain the corresponding topic name. Must match a supported message type exactly.</param>
    /// <returns>The topic name corresponding to the specified message type.</returns>
    /// <exception cref="Exception">Thrown if the specified message type is not supported.</exception>
    private string GetTopicName(string messageType)
    {
        // TODO: This is the wrong type, just for testing right now
        if (messageType.Equals(nameof(FloodReportSourceDeleted), StringComparison.Ordinal))
        {
            return TopicNames.FloodReportSourceDeleted;
        }

        throw new Exception($"Message type {messageType} is not supported. If this is a new message type, please update the {nameof(GetTopicName)} method in the {nameof(Outbox)} project to return the correct topic name.");
    }
}

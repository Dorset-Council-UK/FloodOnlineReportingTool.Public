using Azure.Messaging.ServiceBus;
using FloodOnlineReportingTool.Contracts;
using FloodOnlineReportingTool.Contracts.Topics;
using FloodOnlineReportingTool.Database.DbContexts;
using FloodOnlineReportingTool.Database.Models.Messaging;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Net.Mime;

namespace Outbox;

#pragma warning disable MA0051 // Method is too long

public sealed class Worker(
    ILogger<Worker> logger,
    IConfiguration configuration,
    IServiceProvider serviceProvider,
    IHostEnvironment hostEnvironment,
    ServiceBusClient serviceBusClient
) : BackgroundService
{
    public const string ActivitySourceName = "OutboxWorker";
    private static readonly ActivitySource _activitySource = new(ActivitySourceName);
    private const int BatchSize = 10;

    private const string _source = "FloodOnlineReportingTool.Public";
    private readonly string _environment = hostEnvironment.EnvironmentName;
    private static readonly string _machineName = Environment.MachineName;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Outbox worker started...");

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(10));

        while (await timer.WaitForNextTickAsync(cancellationToken))
        {
            using var activity = _activitySource.StartActivity("Processing outbox messages", ActivityKind.Producer);
            logger.LogDebug("Checking for pending outbox messages");

            try
            {
                // get the pending outbox messages
                await using var scope = serviceProvider.CreateAsyncScope();
                var publicDbContext = scope.ServiceProvider.GetRequiredService<PublicDbContext>();
                List<OutboxMessage> pendingMessages = await publicDbContext.OutboxMessages
                    .Where(m => m.Status == MessageStatus.Pending)
                    .OrderByDescending(m => m.Priority)
                    .ThenBy(m => m.Created) // FIFO processing
                    .Take(BatchSize)
                    .ToListAsync(cancellationToken);

                if (pendingMessages.Count == 0)
                {
                    logger.LogDebug("No pending outbox messages");
                    continue;
                }

                logger.LogInformation("Processing {MessageCount} outbox message(s)", pendingMessages.Count);

                foreach (var outboxMessage in pendingMessages)
                {
                    try
                    {
                        string topicName = GetTopicName(outboxMessage.MessageType);
                        await using ServiceBusSender sender = serviceBusClient.CreateSender(topicName);

                        ServiceBusMessage message = new(outboxMessage.Message)
                        {
                            ContentType = MediaTypeNames.Application.Json,
                            MessageId = outboxMessage.Id.ToString(),
                            Subject = outboxMessage.MessageType,
                            ApplicationProperties =
                            {
                                { "Priority", (int)outboxMessage.Priority },
                                { "Source", _source },
                                { "Environment", _environment },
                                { "MachineName", _machineName },
                            },
                        };

                        await sender.SendMessageAsync(message, cancellationToken);

                        outboxMessage.Status = MessageStatus.Processed;
                        outboxMessage.Delivered = DateTimeOffset.UtcNow;

                        logger.LogInformation("Sent outbox message {MessageId} to topic {TopicName}", outboxMessage.Id, topicName);
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        outboxMessage.Status = MessageStatus.Failed;
                        outboxMessage.ErrorReason = ex.Message;

                        activity?.AddException(ex);
                        logger.LogError(ex, "Failed to send outbox message {MessageId}", outboxMessage.Id);
                    }
                }

                await publicDbContext.SaveChangesAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // Cancellation requested — exit gracefully
                break;
            }
            catch (Exception ex)
            {
                activity?.AddException(ex);
                logger.LogError(ex, "Error processing outbox messages");
            }
        }

        logger.LogInformation("Outbox worker stopping...");
    }

    /// <summary>
    /// Retrieves the topic name associated with the specified message type.
    /// </summary>
    /// <remarks>Optional suffix can be set via configuration (Outbox:TopicSuffix) and will be appended to end of the topic name. Useful for testing.</remarks>
    /// <param name="messageType">The name of the message type for which to obtain the corresponding topic name. Must match a supported message type exactly.</param>
    /// <returns>The topic name corresponding to the specified message type.</returns>
    /// <exception cref="NotSupportedException">Thrown if the specified message type is not supported.</exception>
    private string GetTopicName(string messageType)
    {
        var topicName = messageType switch
        {
            nameof(FloodReportSourceCreated) => TopicNames.FloodReportSourceCreated,
            nameof(FloodReportSourceUpdated) => TopicNames.FloodReportSourceUpdated,
            _ => throw new NotSupportedException($"Message type {messageType} is not supported."),
        };

        string? suffix = configuration["Outbox:TopicSuffix"];
        return string.IsNullOrWhiteSpace(suffix) ? topicName : $"{topicName}{suffix}";
    }
}

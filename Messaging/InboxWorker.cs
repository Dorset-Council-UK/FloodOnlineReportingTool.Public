using Azure.Messaging.ServiceBus;
using Messaging.Consumers;

namespace Messaging;

public sealed class InboxWorker(
    ILogger<InboxWorker> logger,
    IConfiguration configuration,
    IEnumerable<IInboxConsumer> consumers,
    ServiceBusClient serviceBusClient
) : BackgroundService
{
    public const string ActivitySourceName = "InboxWorker";

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        string subscriptionName = configuration["Inbox:SubscriptionName"]
            ?? throw new InvalidOperationException("Missing configuration setting: 'Inbox:SubscriptionName' is required.");

        // Deprecated setting
        if (!string.IsNullOrWhiteSpace(configuration["Outbox:TopicSuffix"]))
        {
            throw new InvalidOperationException("Configuration setting 'Outbox:TopicSuffix' is deprecated. Rename it to 'Messaging:TopicSuffix'.");
        }

        List<ServiceBusProcessor> processors = [];

        try
        {
            // Create a processor for each consumer
            foreach (var consumer in consumers)
            {
                var processor = serviceBusClient.CreateProcessor(consumer.TopicName, subscriptionName, consumer.ProcessorOptions);

                processor.ProcessMessageAsync += consumer.HandleMessage;
                processor.ProcessErrorAsync += consumer.HandleError;

                await processor.StartProcessingAsync(cancellationToken);
                processors.Add(processor);

                logger.LogInformation("Started processor for topic: {Topic}", consumer.TopicName);
            }

            logger.LogInformation("All processors started, waiting for cancellation...");
            await Task.Delay(Timeout.Infinite, cancellationToken);
        }
        finally
        {
            // Graceful shutdown
            foreach (var processor in processors)
            {
                await processor.StopProcessingAsync(cancellationToken);
                await processor.DisposeAsync();
            }
            logger.LogInformation("Service bus processors stopped");
        }
    }
}

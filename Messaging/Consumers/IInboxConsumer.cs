using Azure.Messaging.ServiceBus;

namespace Messaging.Consumers;

public interface IInboxConsumer
{
    /// <summary>
    /// The name of the contract being consumed, used for logging and diagnostics
    /// </summary>
    string ContractName { get; }

    /// <summary>
    /// The full topic name (including optional suffix) this consumer listens to
    /// </summary>
    string TopicName { get; }

    /// <summary>
    /// Processor options to use for configuring the <see cref="ServiceBusProcessor"/>
    /// </summary>
    ServiceBusProcessorOptions ProcessorOptions { get; }

    /// <summary>
    /// The handler responsible for processing messages received from the subscription
    /// </summary>
    Task HandleMessage(ProcessMessageEventArgs args);

    /// <summary>
    /// The handler responsible for processing unhandled exceptions thrown while this processor is running
    /// </summary>
    Task HandleError(ProcessErrorEventArgs args);
}

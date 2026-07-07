using Azure.Messaging.ServiceBus;
using FloodOnlineReportingTool.Contracts;
using FloodOnlineReportingTool.Contracts.Shared;
using FloodOnlineReportingTool.Contracts.Topics;
using FloodOnlineReportingTool.Database.Repositories;
using System.Text.Json;

namespace Messaging.Consumers;

internal sealed class FloodReportExtraInfoRequestConsumer(
    ILogger<FloodReportExtraInfoRequestConsumer> logger,
    IConfiguration configuration,
    IServiceScopeFactory scopeFactory
) : IInboxConsumer
{
    public string ContractName => nameof(FloodReportExtraInfoRequest);

    public string TopicName => $"{TopicNames.FloodReportExtraInfoRequest}{configuration["Messaging:TopicSuffix"]}";

    public ServiceBusProcessorOptions ProcessorOptions => new()
    {
        AutoCompleteMessages = false,
        MaxConcurrentCalls = 2,
    };

    private readonly JsonSerializerOptions _jsonOptions = JsonSerializerOptions.Web;

    public async Task HandleMessage(ProcessMessageEventArgs args)
    {
        logger.LogInformation("Consuming contract: {Contract}", ContractName);

        try
        {
            var message = JsonSerializer.Deserialize<FloodReportExtraInfoRequest>(args.Message.Body, _jsonOptions);
            if (message is null)
            {
                logger.LogError("Reading {Contract} {MessageId} failed", ContractName, args.Message.MessageId);
                await args.DeadLetterMessageAsync(args.Message, "Message could not be read", cancellationToken: args.CancellationToken);
                return;
            }

            //// InboxWorker and Consumers are singletons, so we have to create a scope to our service
            //await using var scope = scopeFactory.CreateAsyncScope();
            //IFloodReportSourceRepository floodReportSourceRepository = scope.ServiceProvider.GetRequiredService<IFloodReportSourceRepository>();
            //var result = await floodReportSourceRepository.Update(message.Reference, RecordStatusIds.ActionNeeded, args.CancellationToken);

            //await args.CompleteMessageAsync(args.Message, args.CancellationToken);

            logger.LogInformation("Processed {Contract} {MessageId}", ContractName, args.Message.MessageId);
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Message could not be deserialized for {Contract} {MessageId}", ContractName, args.Message.MessageId);
            await args.DeadLetterMessageAsync(args.Message, ex.Message, ex.InnerException?.Message ?? "", cancellationToken: args.CancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing {Contract} {MessageId}", ContractName, args.Message.MessageId);
            await args.DeadLetterMessageAsync(args.Message, ex.Message, ex.InnerException?.Message ?? "", cancellationToken: args.CancellationToken);
        }
    }

    public Task HandleError(ProcessErrorEventArgs args)
    {
        logger.LogCritical(args.Exception, "{Contract} error on Entity path: {EntityPath} Source: {ErrorSource}", ContractName, args.EntityPath, args.ErrorSource);
        return Task.CompletedTask;
    }
}

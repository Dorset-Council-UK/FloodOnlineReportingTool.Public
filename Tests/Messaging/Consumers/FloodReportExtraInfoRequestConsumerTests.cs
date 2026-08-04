using Azure.Messaging.ServiceBus;
using FloodOnlineReportingTool.Contracts;
using FloodOnlineReportingTool.Contracts.Topics;
using FloodOnlineReportingTool.Database.Models.Flood;
using FloodOnlineReportingTool.Database.Models.ResultModels;
using FloodOnlineReportingTool.Database.Repositories;
using Messaging.Consumers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Runtime;
using System.Text.Json;

namespace Tests.Messaging.Consumers;

public class FloodReportExtraInfoRequestConsumerTests
{
    private readonly ILogger<FloodReportExtraInfoRequestConsumer> _logger;
    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public FloodReportExtraInfoRequestConsumerTests()
    {
        _logger = Substitute.For<ILogger<FloodReportExtraInfoRequestConsumer>>();
        _configuration = Substitute.For<IConfiguration>();
        _serviceScopeFactory = Substitute.For<IServiceScopeFactory>();
    }

    private void SetupServiceScopeFactory(bool throwError = false)
    {
        var floodReportSource = new FloodReportSource()
        {
            Reference = "REF-123",
            StatusId = Guid.Empty,
        };
        var floodReportSourceRepository = Substitute.For<IFloodReportSourceRepository>();
        floodReportSourceRepository
            .Update(Arg.Any<string>(), Arg.Any<Guid>(), TestContext.Current.CancellationToken)
            .Returns(Task.FromResult(new Result<FloodReportSource?>(IsSuccess: true, Value: floodReportSource, [])));

        var serviceProvider = Substitute.For<IServiceProvider>();
        if (throwError)
        {
            serviceProvider
                .GetService(typeof(IFloodReportSourceRepository))
                .Returns(_ => throw new InvalidOperationException($"No service for type '{nameof(IFloodReportSourceRepository)}' has been registered."));
        }
        else
        {
            serviceProvider
                .GetService(typeof(IFloodReportSourceRepository))
                .Returns(floodReportSourceRepository);
        }

        var scope = Substitute.For<IServiceScope>();
        scope.ServiceProvider.Returns(serviceProvider);
        _serviceScopeFactory.CreateScope().Returns(scope);
    }

    [Fact]
    public Task ContractName_Returns_FloodReportExtraInfoRequestContract_Name()
    {
        // Arrange
        var consumer = new FloodReportExtraInfoRequestConsumer(_logger, _configuration, _serviceScopeFactory);

        // Act
        var contractName = consumer.ContractName;

        // Assert
        Assert.Equal(nameof(FloodReportExtraInfoRequest), contractName);
        return Task.CompletedTask;
    }

    [Theory]
    [InlineData(null, TopicNames.FloodReportExtraInfoRequest)]
    [InlineData("", TopicNames.FloodReportExtraInfoRequest)]
    [InlineData("  ", TopicNames.FloodReportExtraInfoRequest)]
    [InlineData("-testing", $"{TopicNames.FloodReportExtraInfoRequest}-testing")]
    [InlineData("  -testing", $"{TopicNames.FloodReportExtraInfoRequest}-testing")]
    [InlineData("-testing  ", $"{TopicNames.FloodReportExtraInfoRequest}-testing")]
    public Task TopicName_Returns_ExpectedTopicName_ForConfiguredSuffix(string? suffix, string expectedTopicName)
    {
        // Arrange
        _configuration["Messaging:TopicSuffix"].Returns(suffix);
        var consumer = new FloodReportExtraInfoRequestConsumer(_logger, _configuration, _serviceScopeFactory);

        // Act
        var topicName = consumer.TopicName;

        // Assert
        Assert.Equal(expectedTopicName, topicName);
        return Task.CompletedTask;
    }

    [Fact]
    public async Task HandleMessage_DeadLetter_WhenJsonDeserializationNull()
    {
        // Arrange
        var consumer = new FloodReportExtraInfoRequestConsumer(_logger, _configuration, _serviceScopeFactory);

        var message = ServiceBusModelFactory.ServiceBusReceivedMessage(body: BinaryData.FromString("null"));
        var args = Substitute.For<ProcessMessageEventArgs>(message, Substitute.For<ServiceBusReceiver>(), TestContext.Current.CancellationToken);
        args.DeadLetterMessageAsync(default, default, default, TestContext.Current.CancellationToken).ReturnsForAnyArgs(Task.CompletedTask);
        args.CompleteMessageAsync(default, TestContext.Current.CancellationToken).ReturnsForAnyArgs(Task.CompletedTask);

        // Act
        await consumer.HandleMessage(args);

        // Assert
        await args.Received(1).DeadLetterMessageAsync(message, "Message could not be read", Arg.Any<string>(), TestContext.Current.CancellationToken);
        await args.DidNotReceiveWithAnyArgs().CompleteMessageAsync(default, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task HandleMessage_DeadLetter_WhenJsonDeserializationThrows()
    {
        // Arrange
        var consumer = new FloodReportExtraInfoRequestConsumer(_logger, _configuration, _serviceScopeFactory);

        var message = ServiceBusModelFactory.ServiceBusReceivedMessage(body: null);
        var args = Substitute.For<ProcessMessageEventArgs>(message, Substitute.For<ServiceBusReceiver>(), TestContext.Current.CancellationToken);
        args.DeadLetterMessageAsync(default, default, default, TestContext.Current.CancellationToken).ReturnsForAnyArgs(Task.CompletedTask);
        args.CompleteMessageAsync(default, TestContext.Current.CancellationToken).ReturnsForAnyArgs(Task.CompletedTask);

        // Act
        await consumer.HandleMessage(args);

        // Assert
        await args.Received(1).DeadLetterMessageAsync(message, "JSON exception", Arg.Any<string>(), TestContext.Current.CancellationToken);
        await args.DidNotReceiveWithAnyArgs().CompleteMessageAsync(default, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task HandleMessage_DeadLetter_WhenHandlerThrows()
    {
        // Arrange
        SetupServiceScopeFactory(throwError: true);
        var consumer = new FloodReportExtraInfoRequestConsumer(_logger, _configuration, _serviceScopeFactory);

        var floodReportExtraInfoRequest = new FloodReportExtraInfoRequest("REF-123", DateTimeOffset.UtcNow);
        var message = ServiceBusModelFactory.ServiceBusReceivedMessage(body: BinaryData.FromObjectAsJson(floodReportExtraInfoRequest, JsonSerializerOptions.Web));
        var args = Substitute.For<ProcessMessageEventArgs>(message, Substitute.For<ServiceBusReceiver>(), TestContext.Current.CancellationToken);
        args.DeadLetterMessageAsync(default, default, default, TestContext.Current.CancellationToken).ReturnsForAnyArgs(Task.CompletedTask);
        args.CompleteMessageAsync(default, TestContext.Current.CancellationToken).ReturnsForAnyArgs(Task.CompletedTask);

        // Act
        await consumer.HandleMessage(args);

        // Assert
        await args.Received(1).DeadLetterMessageAsync(message, "Unexpected exception", $"No service for type '{nameof(IFloodReportSourceRepository)}' has been registered.", TestContext.Current.CancellationToken);
        await args.DidNotReceiveWithAnyArgs().CompleteMessageAsync(default, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task HandleMessage_CompleteMessage_WhenPayloadValid()
    {
        // Arrange
        SetupServiceScopeFactory();
        var consumer = new FloodReportExtraInfoRequestConsumer(_logger, _configuration, _serviceScopeFactory);

        var floodReportExtraInfoRequest = new FloodReportExtraInfoRequest("REF-123", DateTimeOffset.UtcNow);
        var message = ServiceBusModelFactory.ServiceBusReceivedMessage(body: BinaryData.FromObjectAsJson(floodReportExtraInfoRequest, JsonSerializerOptions.Web));
        var args = Substitute.For<ProcessMessageEventArgs>(message, Substitute.For<ServiceBusReceiver>(), TestContext.Current.CancellationToken);
        args.CompleteMessageAsync(default, TestContext.Current.CancellationToken).ReturnsForAnyArgs(Task.CompletedTask);

        // Act
        await consumer.HandleMessage(args);

        // Assert
        await args.Received(1).CompleteMessageAsync(message, TestContext.Current.CancellationToken);
    }
}

using FloodOnlineReportingTool.Public.Services;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace Tests.Notifications;

public class GovUKNotifyTests
{

    [Fact]
    public async Task SendTestNotification()
    {
        // Get the project secrets from main Blazor project
        var builder = new ConfigurationBuilder().AddUserSecrets<GovUKNotifyTests>();
        var settings = builder.Build();

        // Arrange
        var floodReportService = Substitute.For<IGovNotifyEmailSender>();
        floodReportService.SendTestNotification(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("ok"));
        string secretValue = settings["TestEmail"] ?? "Failure";

        // Act
        var result = await floodReportService.SendTestNotification(
            secretValue,
            "This is a test of the FORT notification system - public reporting project.",
            CancellationToken.None);

        // Assert
        Assert.Equal("ok", result);

    }
}

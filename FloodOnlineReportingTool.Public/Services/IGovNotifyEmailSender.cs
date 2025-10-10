namespace FloodOnlineReportingTool.Public.Services;

public interface IGovNotifyEmailSender
{
    Task<string> SendAccountActivationNotification(Guid recordId, string recordReference, string recordPassword, string locationDescription, double easting, double northing, DateTimeOffset reportDate);

    Task<string> SendTestNotification(string targetEmail, string testMessage, CancellationToken ct);
}

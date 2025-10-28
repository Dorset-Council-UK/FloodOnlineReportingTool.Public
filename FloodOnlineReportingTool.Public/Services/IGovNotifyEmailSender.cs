namespace FloodOnlineReportingTool.Public.Services;

public interface IGovNotifyEmailSender
{
    // Account notifications
    Task<string> SendAccountActivationNotification(Guid recordId, bool temporaryAccessOnly, string recordReference, string locationDescription, double easting, double northing, DateTimeOffset reportDate);

    // Contact notifications
    Task<string> SendContactUpdatedNotification(string contactEmail, string contactPhone, string contactDisplayName, string reportReference, string contactType);
    Task<string> SendContactDeletedNotification(string contactEmail, string contactDisplayName, string reportReference, string contactType);

    // Test notifications
    Task<string> SendTestNotification(string targetEmail, string testMessage, CancellationToken ct);
}

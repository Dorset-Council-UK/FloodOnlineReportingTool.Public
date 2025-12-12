namespace FloodOnlineReportingTool.Public.Services;

public interface IGovNotifyEmailSender
{
    // Report submitted notifications
    Task<string> SendReportSubmittedNotification(bool isPrimary, bool temporaryAccessOnly, string recordReference, string contactDisplayName, string contactEmail, string contactPhone, string locationDescription, double easting, double northing, DateTimeOffset reportDate);

    // Account notifications
    Task<string> SendEmailVerificationNotification(string contactEmail, string contactDisplayName, int? verificationCode, DateTimeOffset verificationExpiryUtc);
    Task<string> SendEmailVerificationLinkNotification(string contactEmail, string contactDisplayName, int? verificationCode, DateTimeOffset verificationExpiryUtc);

    // Contact notifications
    Task<string> SendContactDeletedNotification(string contactType, string contactEmail, string contactDisplayName, string recordReference);

    // Test notifications
    Task<string> SendTestNotification(string targetEmail, string testMessage, CancellationToken ct);
}

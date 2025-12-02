namespace FloodOnlineReportingTool.Public.Services;

public interface IGovNotifyEmailSender
{
    Task<string> SendReportSubmittedNotification(bool isPrimary, bool temporaryAccessOnly, string recordReference, string contactDisplayName, string contactEmail, string contactPhone, string locationDescription, double easting, double northing, DateTimeOffset reportDate);

    // Contact notifications
    // The create contact is a special case where we verify the contact email but also notify them of the particulars of the record and how it can be edited.
    Task<string> SendEmailVerificationNotification(string contactEmail, string contactDisplayName, int? verificationCode, DateTimeOffset verificationExpiryUtc);
    // Regular contact notifications
    Task<string> SendContactUpdatedNotification(string contactType, string contactEmail, string contactPhone, string contactDisplayName, string recordReference);
    Task<string> SendContactDeletedNotification(string contactType, string contactEmail, string contactDisplayName, string recordReference);

    // Record notifications
    //Task<string> SendEmailVerificationNotification(string contactType, bool isPrimary, bool temporaryAccessOnly, string contactEmail, string contactPhone, string contactDisplayName, string recordReference, string locationDescription, double easting, double northing, DateTimeOffset reportDate);

    // Test notifications
    Task<string> SendTestNotification(string targetEmail, string testMessage, CancellationToken ct);
}

namespace FloodOnlineReportingTool.Public.Services;

public interface IGovNotifyEmailSender
{
    // Contact notifications
    // The create contact is a special case where we verify the contact email but also notify them of the particulars of the record and how it can be edited.
    Task<string> SendEmailVerificationNotification(string contactType, bool isPrimary, bool temporaryAccessOnly, string contactEmail, string contactPhone, string contactDisplayName, string recordReference, string locationDescription, double easting, double northing, DateTimeOffset reportDate);
    // Regular contact notifications
    Task<string> SendContactUpdatedNotification(string contactType, string contactEmail, string contactPhone, string contactDisplayName, string recordReference);
    Task<string> SendContactDeletedNotification(string contactType, string contactEmail, string contactDisplayName, string recordReference);

    // Test notifications
    Task<string> SendTestNotification(string targetEmail, string testMessage, CancellationToken ct);
}

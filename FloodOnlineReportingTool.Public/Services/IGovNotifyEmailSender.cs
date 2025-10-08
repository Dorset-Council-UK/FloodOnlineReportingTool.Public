namespace FloodOnlineReportingTool.Public.Services;

internal interface IGovNotifyEmailSender
{
    Task<string> SendAccountActivationNotification(Guid recordId, string recordReference, string recordPassword, string locationDescription, double easting, double northing, DateTimeOffset reportDate);
}

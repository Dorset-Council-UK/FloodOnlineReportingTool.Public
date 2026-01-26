using FloodOnlineReportingTool.Database.Compliance;
using FloodOnlineReportingTool.Database.Models.Contact.Subscribe;
using Microsoft.Extensions.Logging;

namespace FloodOnlineReportingTool.Public;

internal static partial class Logger
{
    /// <summary>
    /// Logs subscriber record details with automatic redaction of personal data.
    /// </summary>
    public static void LogSubscriberRecord(this ILogger logger, SubscribeRecord subscriber)
    {
        LogSubscriberRecordInternal(
            logger,
            subscriber.Id,
            subscriber.IsRecordOwner,
            subscriber.ContactType.ToString(),
            subscriber.ContactName,
            subscriber.EmailAddress,
            subscriber.PhoneNumber,
            subscriber.IsEmailVerified,
            subscriber.IsSubscribed);
    }

    [LoggerMessage(0, LogLevel.Information,
        "Subscriber Details: Id={Id}, Owner={IsRecordOwner}, Type={ContactType}, Name={ContactName}, Email={EmailAddress}, Phone={PhoneNumber}, Verified={IsEmailVerified}, Subscribed={IsSubscribed}")]
    private static partial void LogSubscriberRecordInternal(
        ILogger logger,
        [Pii] Guid Id,
        bool IsRecordOwner,
        string ContactType,
        [Pii] string ContactName,
        [Pii] string EmailAddress,
        [Pii] string? PhoneNumber,
        bool IsEmailVerified,
        bool IsSubscribed);
}



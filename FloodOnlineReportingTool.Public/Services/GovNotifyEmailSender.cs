using FloodOnlineReportingTool.Public.Options;
using Microsoft.Extensions.Options;
using Notify.Client;
using System.Globalization;

namespace FloodOnlineReportingTool.Public.Services;

internal class GovNotifyEmailSender(
    ILogger<GovNotifyEmailSender> logger,
    IOptions<GovNotifyOptions> options,
    IWebHostEnvironment environment,
    ICurrentUserService currentUserService,
    NotificationClient notificationClient
) : IGovNotifyEmailSender
{
    private readonly GovNotifyOptions _govNotifyOptions = options.Value;

    /// <summary>
    ///     <para>Send an email to the specified email address via GovNotify.</para>
    ///     <para>If there is a problem pass the exception back up.</para>
    /// </summary>
    /// <remarks>
    /// Error codes are documented at <see href="https://docs.notifications.service.gov.uk/net.html#send-an-email-error-codes">Error Codes</see>
    /// </remarks>
    private async Task<string> SendEmail(string emailAddress, string templateId, Dictionary<string, dynamic>? personalisation, string? clientReference = null, string? emailReplyToId = null, string? oneClickUnsubscribeURL = null)
    {
        var response = await notificationClient
            .SendEmailAsync(emailAddress, templateId, personalisation, clientReference, emailReplyToId, oneClickUnsubscribeURL);

        logger.LogInformation("Email sent with GovNotify response ID {ResponseId}", response.id);

        return response.id;
    }

    private string PublicReportsUrl()
    {
        if (environment.IsDevelopment())
        {
            return "https://localhost:7112/floodriskmanagement";
        }

        if (environment.IsStaging())
        {
            return "https://gi-staging.dorsetcouncil.gov.uk/floodriskmanagement";
        }

        return "https://gi.dorsetcouncil.gov.uk/floodriskmanagement";
    }

    private string DorsetExplorerUrl(int zoom, double easting, double northing)
    {
        var host = environment.IsDevelopment() ? "https://gi-staging.dorsetcouncil.gov.uk/dorsetexplorer" : "https://gi.dorsetcouncil.gov.uk/dorsetexplorer";
        return string.Create(CultureInfo.InvariantCulture, $"{host}#map={zoom}/{easting}/{northing}/0/27700");
    }

    private string GetUsermailAddress()
    {
        if (environment.IsDevelopment() && currentUserService.IsAuthenticated)
        {
            return currentUserService.Email;
        }

        return "";
    }

    public async Task<string> SendTestNotification(string targetEmail, string testMessage, CancellationToken ct)
    {
        var personalisation = new Dictionary<string, dynamic>(StringComparer.CurrentCulture)
        {
            { "from_development", environment.IsDevelopment() },
            { "test_message", testMessage },
        };

        return await SendEmail(targetEmail, _govNotifyOptions.Templates.TestNotification, personalisation);
    }

    // Report submitted notifications

    /// <summary>
    /// This triggers an email when the report has been submitted and is a summary of the flood report. 
    /// It also give the user a link to edit details if needed.
    /// </summary>
    public async Task<string> SendReportSubmittedNotification(bool isRecordOwner, bool canEdit, string recordReference, string contactType, string contactDisplayName, string contactEmail, string locationDescription, double easting, double northing, DateTimeOffset reportDate)
    {
        var personalisation = new Dictionary<string, dynamic>(StringComparer.CurrentCulture)
        {
            { "from_development", environment.IsDevelopment() },
            { "record_owner", isRecordOwner },
            { "can_edit", canEdit },
            { "contact_type", contactType  },
            { "display_name", contactDisplayName },
            { "contactEmail", contactEmail },
            { "record_reference", recordReference },
            { "edit_url", $"[edit your report]({PublicReportsUrl()}/flood-event/{recordReference})" },
            { "flood_location_url", $"[on Dorset Explorer]({DorsetExplorerUrl(17, easting, northing)})" },
            { "location_description", locationDescription},
            { "report_date", reportDate.GdsReadable() },
        };
        var emailAddress = contactEmail;
        if (emailAddress == null)
        {
            return string.Empty;
        }
        return await SendEmail(emailAddress, _govNotifyOptions.Templates.ReportSubmitted, personalisation);
    }

    public async Task<string> SendReportSubmittedCopyNotification(string recordReference, string contactType, string contactDisplayName, string contactEmail, string locationDescription, double easting, double northing, DateTimeOffset reportDate)
    {
        var personalisation = new Dictionary<string, dynamic>(StringComparer.CurrentCulture)
        {
            { "from_development", environment.IsDevelopment() },
            { "contact_type", contactType  },
            { "display_name", contactDisplayName },
            { "contactEmail", contactEmail },
            { "record_reference", recordReference },
            { "edit_url", $"[edit your report]({PublicReportsUrl()}/flood-event/{recordReference})" },
            { "flood_location_url", $"[on Dorset Explorer]({DorsetExplorerUrl(17, easting, northing)})" },
            { "location_description", locationDescription},
            { "report_date", reportDate.GdsReadable() },
        };
        var emailAddress = contactEmail;
        if (emailAddress == null)
        {
            return string.Empty;
        }
        return await SendEmail(emailAddress, _govNotifyOptions.Templates.ReportSubmittedCopy, personalisation);

    }

    // Account notifications

    /// <summary>
    /// This triggers an email to ask the user or contact to verify their email address.
    /// </summary>
    /// <returns></returns>
    public async Task<string> SendEmailVerificationNotification(string contactEmail, string contactDisplayName, int? verificationCode, DateTimeOffset verificationExpiryUtc)
    {
        if (verificationCode is null)
        {
            return string.Empty;
        }

        var personalisation = new Dictionary<string, dynamic>(StringComparer.CurrentCulture)
        {
            { "from_development", environment.IsDevelopment() },
            { "contactDisplayName", contactDisplayName },
            { "VerifyCode", verificationCode },
            { "VerifyUntil", verificationExpiryUtc },
        };

        var emailAddress = contactEmail;
        if (emailAddress == null)
        {
            return string.Empty;
        }
        return await SendEmail(emailAddress, _govNotifyOptions.Templates.VerifyEmailAddress, personalisation);
    }

    /// <summary>
    /// This triggers an email to ask the user or contact to verify their email address by link.
    /// </summary>
    /// <returns></returns>
    public async Task<string> SendEmailVerificationLinkNotification(string contactEmail, string contactDisplayName, string requesterName, string verificationLink, DateTimeOffset verificationExpiryUtc)
    {
        var personalisation = new Dictionary<string, dynamic>(StringComparer.CurrentCulture)
        {
            { "from_development", environment.IsDevelopment() },
            { "contactDisplayName", contactDisplayName },
            { "requesterName", requesterName },
            { "VerifyLink", verificationLink },
            { "VerifyUntil", verificationExpiryUtc },
        };

        var emailAddress = contactEmail;
        if (emailAddress == null)
        {
            return string.Empty;
        }
        return await SendEmail(emailAddress, _govNotifyOptions.Templates.VerifyEmailLinkAddress, personalisation);
    }

    /// <summary>
    /// This triggers an email to a non-user additional contact. The user will get a magic link 
    /// Expiry is 3 days
    /// </summary>
    /// <returns></returns>
    public async Task<string> SendEmailVerificationLinkNotification(string contactEmail, string contactDisplayName, int? verificationCode, DateTimeOffset verificationExpiryUtc)
    {
        if (verificationCode is null)
        {
            return string.Empty;
        }

        var personalisation = new Dictionary<string, dynamic>(StringComparer.CurrentCulture)
        {
            { "from_development", environment.IsDevelopment() },
            { "contactDisplayName", contactDisplayName },
            { "VerifyCode", verificationCode },
            { "VerifyUntil", verificationExpiryUtc },
        };

        var emailAddress = contactEmail;
        if (emailAddress == null)
        {
            return string.Empty;
        }
        return await SendEmail(emailAddress, _govNotifyOptions.Templates.VerifyEmailLinkAddress, personalisation);
    }

    // Contact notifications

    // TODO: Update this notification template as required - should include instructions in case this was not intended as deletion removes the ability to self fix.
    /// <summary>
    /// This triggers an email to notify a contact that their details have been deleted.
    /// </summary>
    public async Task<string> SendContactDeletedNotification(string contactType, string contactEmail, string contactDisplayName, string recordReference)
    {
        var personalisation = new Dictionary<string, dynamic>(StringComparer.CurrentCulture)
        {
            { "from_development", environment.IsDevelopment() },
            { "recordReference", recordReference },
            { "contactDisplayName", contactDisplayName },
            { "contactType", contactType },
        };

        var emailAddress = GetUsermailAddress();
        if (emailAddress == null)
        {
            return string.Empty;
        }
        return await SendEmail(emailAddress, _govNotifyOptions.Templates.ConfirmContactDeleted, personalisation);
    }
}

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
    private readonly GovNotifyOptions _govNotifySettings = options.Value;

    /// <summary>
    ///     <para>Send an email to the specified email address via GovNotify.</para>
    ///     <para>If there is a problem pass the exception back up.</para>
    /// </summary>
    /// <remarks>
    /// Error codes are documented at <see href="https://docs.notifications.service.gov.uk/net.html#send-an-email-error-codes">Error Codes</see>
    /// </remarks>
    private async Task<string> SendEmail(string emailAddress, string templateId, Dictionary<string, dynamic>? personalisation, string? clientReference = null, string? emailReplyToId = null, string? oneClickUnsubscribeURL = null)
    {
        logger.LogDebug("Sending email to {EmailAddress}", emailAddress);

        var response = await notificationClient
            .SendEmailAsync(emailAddress, templateId, personalisation, clientReference, emailReplyToId, oneClickUnsubscribeURL);

        logger.LogInformation("Email sent to {EmailAddress} with GovNotify response ID {ResponseId}", emailAddress, response.id);

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

        return await SendEmail(targetEmail, _govNotifySettings.Templates.TestNotification, personalisation);
    }

    // Account notifications
    // TODO: Update this notification template as required.
    /// <summary>
    /// This triggers an email to ask the user or contact to verify their email address.
    /// If isPrimary then the user will have some permission level. 
    /// For example, if temporaryAccessOnly is true then the email should explain that this access 
    /// is limited to the current session. If false then the user is using a persistent account.
    /// If isPrimary is false then the contact will recieve notifications if they verify their email.
    /// </summary>
    public async Task<string> SendEmailVerificationNotification(string contactType, bool isPrimary, bool temporaryAccessOnly, string contactEmail, string contactPhone, string contactDisplayName, string recordReference, string locationDescription, double easting, double northing, DateTimeOffset reportDate)
    {
        var personalisation = new Dictionary<string, dynamic>(StringComparer.CurrentCulture)
        {
            { "from_development", environment.IsDevelopment() },
            { "recordReference", recordReference },
            { "isPrimary", isPrimary },
            { "temporaryAccessOnly", temporaryAccessOnly },
            { "contactDisplayName", contactDisplayName },
            { "contactPhone", contactPhone },
            { "contactType", contactType },
            { "edit_url", $"[edit your report]({PublicReportsUrl()}/flood-event/{recordReference})" },
            { "flood_location_url", $"[on Dorset Explorer]({DorsetExplorerUrl(17, easting, northing)})" },
            { "location_description", locationDescription},
            { "report_date", reportDate.GdsReadable() },
        };

        var emailAddress = GetUsermailAddress();
        if (emailAddress == null)
        {
            return string.Empty;
        }
        return await SendEmail(emailAddress, _govNotifySettings.Templates.VerifyEmailAddress, personalisation);
    }

    // Contact notifications
    // TODO: Create / set notification template.
    /// <summary>
    /// This triggers an email to notify a contact that their details have been updated.
    /// </summary>
    public async Task<string> SendContactUpdatedNotification(string contactType, string contactEmail, string contactPhone, string contactDisplayName, string recordReference)
    {
        var personalisation = new Dictionary<string, dynamic>(StringComparer.CurrentCulture)
        {
            { "from_development", environment.IsDevelopment() },
            { "recordReference", recordReference },
            { "contactDisplayName", contactDisplayName },
            { "contactPhone", contactPhone },
            { "contactType", contactType },
        };

        var emailAddress = GetUsermailAddress();
        if (emailAddress == null)
        {
            return string.Empty;
        }
        return await SendEmail(emailAddress, _govNotifySettings.Templates.ConfirmContactUpdated, personalisation);
    }

    // TODO: Create / set notification template.
    /// <summary>
    /// This triggers an email to notify a contact that their details have been deleted.
    /// The notification should include instructions in case this was not intended as deletion removes the ability to self fix.
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
        return await SendEmail(emailAddress, _govNotifySettings.Templates.ConfirmContactDeleted, personalisation);
    }
}

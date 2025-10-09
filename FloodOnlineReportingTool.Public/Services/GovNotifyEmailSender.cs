using FloodOnlineReportingTool.Public.Settings;
using Microsoft.Extensions.Options;
using Notify.Client;
using System.Globalization;
using System.Net.Mail;

namespace FloodOnlineReportingTool.Public.Services;

internal class GovNotifyEmailSender(
    ILogger<GovNotifyEmailSender> logger,
    IOptions<GovNotifySettings> options,
    IWebHostEnvironment environment,
    ICurrentUserService currentUserService,
    NotificationClient notificationClient
) : IGovNotifyEmailSender
{
    private readonly GovNotifySettings _govNotifySettings = options.Value;

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
            .SendEmailAsync(emailAddress, templateId, personalisation, clientReference, emailReplyToId, oneClickUnsubscribeURL)
            .ConfigureAwait(false);

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

        return await SendEmail(targetEmail, _govNotifySettings.Templates.TestNotification, personalisation).ConfigureAwait(false);
    }

    public async Task<string> SendAccountActivationNotification(Guid recordId, string recordReference, string recordPassword, string locationDescription, double easting, double northing, DateTimeOffset reportDate)
    {
        var personalisation = new Dictionary<string, dynamic>(StringComparer.CurrentCulture)
        {
            { "from_development", environment.IsDevelopment() },
            { "recordReference", recordReference },
            { "recordPassword", recordPassword },
            { "edit_url", $"[edit your report]({PublicReportsUrl()}/flood-event/{recordId})" },
            { "flood_location_url", $"[on Dorset Explorer]({DorsetExplorerUrl(17, easting, northing)})" },
            { "location_description", locationDescription},
            { "report_date", reportDate.GdsReadable() },
        };

        var emailAddress = GetUsermailAddress();
        if (emailAddress == null)
        {
            return string.Empty;
        }
        return await SendEmail(emailAddress, _govNotifySettings.Templates.AccountActivationNotification, personalisation).ConfigureAwait(false);
    }
}

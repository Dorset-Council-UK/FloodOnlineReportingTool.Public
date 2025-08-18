using FloodOnlineReportingTool.Database.Models;
using FloodOnlineReportingTool.Database.Settings;
using GdsBlazorComponents;
using FloodOnlineReportingTool.Public.Models.Order;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Text;

namespace FloodOnlineReportingTool.Public.Components.Pages.Account;

public partial class EmailConfirm(
    ILogger<EmailConfirm> logger,
    NavigationManager navigationManager,
    UserManager<FortUser> userManager,
    IOptions<GISSettings> gisSettings
) : IPageOrder
{
    // Page order properties
    public string Title { get; set; } = AccountPages.EmailConfirm.Title;
    public IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [];

    [SupplyParameterFromQuery]
    private string? ReturnUrl { get; set; }

    [SupplyParameterFromQuery]
    private string? UserId { get; set; }

    [SupplyParameterFromQuery]
    private string? Code { get; set; }

    private readonly string _signInTitle = AccountPages.SignIn.Title.ToLowerInvariant();
    private string _signInUrl = "";
    private string _resendEmailUrl = "";
    private bool _confirmed = false;
    private bool _confirmError = false;
    private string? _expirationMessage;
    private GISSettings GisSettings => gisSettings.Value;

    protected override void OnInitialized()
    {
        _signInUrl = UrlWithRedirect(AccountPages.SignIn.Url);
        _resendEmailUrl = UrlWithRedirect(AccountPages.EmailResendConfirm.Url);
        _expirationMessage = CreateExpirationMessage();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await ConfirmEmail();
        }
    }

    private string UrlWithRedirect(string url)
    {
        var parameters = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            { "returnUrl", ReturnUrl ?? GeneralPages.Home.Url },
        };
        return navigationManager.GetUriWithQueryParameters(url, parameters);
    }

    private string DecodeCode(string code)
    {
        try
        {
            return Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        }
        catch (FormatException ex)
        {
            logger.LogError(ex, "Error decoding confirmation code");
        }
        return string.Empty;
    }

    /// <summary>
    /// Create a human-readable message for access expiration
    /// </summary>
    private string? CreateExpirationMessage()
    {
        var now = DateTimeOffset.UtcNow;
        var expiration = now.AddMonths(GisSettings.AccessTokenIssueDurationMonths);
        var timeLeft = expiration - now;
        return string.Format(CultureInfo.CurrentCulture, "Your access to this flood report will expire in {0}.", timeLeft.GdsReadable());
    }

    private async Task ConfirmEmail()
    {
        if (UserId == null)
        {
            logger.LogDebug("Cannot confirm email, UserId is null");
            return;
        }

        if (Code == null)
        {
            logger.LogDebug("Cannot confirm email, Code is null");
            return;
        }

        var user = await userManager.FindByIdAsync(UserId);
        if (user == null)
        {
            logger.LogError("Cannot confirm email, user not found: {UserId}", UserId);
            _confirmError = true;
            StateHasChanged();
            return;
        }

        var result = await userManager.ConfirmEmailAsync(user, DecodeCode(Code));
        _confirmed = result.Succeeded;
        _confirmError = result.Errors.Any();
        if (_confirmed)
        {
            logger.LogInformation("User email confirmed: {UserId} {Result}", user.Id, result.ToString());
        }
        else
        {
            logger.LogError("Error confirming email: {UserId} {Result}", user.Id, result.ToString());
        }
        StateHasChanged();
    }
}

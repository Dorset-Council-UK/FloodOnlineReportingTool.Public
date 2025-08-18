using FloodOnlineReportingTool.Database.Models;
using FloodOnlineReportingTool.Public.Models.Order;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity;

namespace FloodOnlineReportingTool.Public.Components.Pages.Account;

// Static server-side rendering (static SSR)
[ExcludeFromInteractiveRouting]
public partial class SignIn(
    ILogger<SignIn> logger,
    NavigationManager navigationManager,
    SignInManager<FortUser> signInManager,
    IGdsJsInterop gdsJs
) : IPageOrder, IAsyncDisposable
{
    // Page order properties
    public string Title { get; set; } = AccountPages.SignIn.Title;
    public IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [];

    [SupplyParameterFromQuery]
    private string? ReturnUrl { get; set; }

    [SupplyParameterFromForm]
    private Models.Account.SignIn Model { get; set; } = default!;

    private EditContext _editContext = default!;
    private readonly CancellationTokenSource _cts = new();
    private ValidationMessageStore? _messageStore;
    private readonly IReadOnlyCollection<GdsOptionItem<bool>> _rememberMeOptions = [
        new("remember-me", "Remember me", value: true),
    ];
    private string _forgotPasswordUrl = "";
    private string _registerUrl = "";
    private string _resendEmailUrl = "";

    public async ValueTask DisposeAsync()
    {
        try
        {
            await _cts.CancelAsync();
            _cts.Dispose();
        }
        catch (Exception)
        {
        }

        GC.SuppressFinalize(this);
    }

    protected override void OnInitialized()
    {
        _forgotPasswordUrl = UrlWithRedirect(AccountPages.PasswordForgot.Url);
        _registerUrl = UrlWithRedirect(AccountPages.Register.Url);
        _resendEmailUrl = UrlWithRedirect(AccountPages.EmailResendConfirm.Url);

        // Setup model and edit context
        Model ??= new();
        _editContext = new(Model);
        _editContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
        _messageStore = new(_editContext);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await gdsJs.InitGds(_cts.Token);
        }
    }

    private async Task Submit()
    {
        logger.LogDebug("Sign in form submitted");

        _messageStore?.Clear();

        if (!_editContext.Validate())
        {
            return;
        }

        // Safety check, which should not be needed
        if (Model.Email == null || Model.Password == null)
        {
            return;
        }
        await SignInUser(Model.Email, Model.Password);
    }

    private async Task SignInUser(string userName, string password)
    {
        // Keep the details the user sees to a minimum
        var emailFieldIdentifier = FieldIdentifier.Create(() => Model.Email);

        try
        {
            logger.LogDebug("Signing the user in");

            // Sign in with a cookie
            var signInResult = await signInManager.PasswordSignInAsync(userName, password, Model.RememberMe, lockoutOnFailure: true).ConfigureAwait(false);

            var result = signInResult.ToFortSignInResult(ReturnUrl, logger);
            if (result.Succeeded || result.ShouldRedirect)
            {
                navigationManager.NavigateToLocal(result.RedirectUrl);
            }

            await InvokeAsync(() =>
            {
                _messageStore?.Add(emailFieldIdentifier, result.ErrorMessage);
                _editContext.NotifyValidationStateChanged();
                StateHasChanged();
            });
            return;
        }
        catch (NavigationException)
        {
            // We ignore this with a SSR component, but MUST let it happen
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error signing in user");
        }

        await InvokeAsync(() =>
        {
            _messageStore?.Add(emailFieldIdentifier, "Unable to sign in, please try again");
            _editContext.NotifyValidationStateChanged();
            StateHasChanged();
        });
    }

    private string UrlWithRedirect(string url)
    {
        var parameters = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            { "returnUrl", ReturnUrl },
        };
        return navigationManager.GetUriWithQueryParameters(url, parameters);
    }
}

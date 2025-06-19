using FloodOnlineReportingTool.DataAccess.Models;
using FloodOnlineReportingTool.Public.Models.Order;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace FloodOnlineReportingTool.Public.Components.Pages.Account;

// Static server-side rendering (static SSR) - Because of the automatic sign in used below in this page
[ExcludeFromInteractiveRouting]
public partial class Register(
    ILogger<Register> logger,
    UserManager<FortUser> userManager,
    NavigationManager navigationManager,
    SignInManager<FortUser> signInManager,
    IUserStore<FortUser> userStore,
    IEmailSender<FortUser> emailSender,
    IGdsJsInterop gdsJs
) : IPageOrder, IAsyncDisposable
{
    // Page order properties
    public string Title { get; set; } = AccountPages.Register.Title;
    public IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [];

    [SupplyParameterFromQuery]
    private string? ReturnUrl { get; set; }

    [SupplyParameterFromForm]
    private Models.Account.Register Model { get; set; } = default!;

    private EditContext _editContext = default!;
    private readonly CancellationTokenSource _cts = new();
    private ValidationMessageStore? _messageStore;

    protected override void OnInitialized()
    {
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

    private async Task Submit()
    {
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
        await RegisterUser(Model.Email, Model.Password);
    }

    private async Task RegisterUser(string email, string password)
    {
        logger.LogDebug("Registering user");

        var emailStore = (IUserEmailStore<FortUser>)userStore;

        var user = new FortUser();
        await userStore.SetUserNameAsync(user, email, _cts.Token);
        await emailStore.SetEmailAsync(user, email, _cts.Token);
        var result = await userManager.CreateAsync(user, password);

        logger.LogInformation("User registration: {Result}", result.ToString());
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                _messageStore?.Add(GetBestErrorField(error), error.Description);
            }
            await InvokeAsync(() => _editContext.NotifyValidationStateChanged());
            return;
        }

        var confirmationLink = await ConfirmationLink(user);
        await emailSender.SendConfirmationLinkAsync(user, email, confirmationLink);

        if (userManager.Options.SignIn.RequireConfirmedEmail)
        {
            navigationManager.NavigateTo(AccountPages.EmailConfirm.Url);
            return;
        }

        await SignInUser(user);
    }

    private FieldIdentifier GetBestErrorField(IdentityError error)
    {
        var emailField = FieldIdentifier.Create(() => Model.Email);
        var passwordField = FieldIdentifier.Create(() => Model.Password);
        const string password = "password";

        if (error.Code.Contains(password, StringComparison.OrdinalIgnoreCase))
        {
            return passwordField;
        }

        if (error.Description.Contains(password, StringComparison.OrdinalIgnoreCase))
        {
            return passwordField;
        }

        return emailField;
    }

    private async Task<string> ConfirmationLink(FortUser user)
    {
        var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        var parameters = new Dictionary<string, object?>(StringComparer.CurrentCulture)
        {
            { "userId", user.Id },
            { "code", code },
        };
        var uri = navigationManager.ToAbsoluteUri(AccountPages.EmailConfirm.Url);
        return navigationManager.GetUriWithQueryParameters(uri.ToString(), parameters);
    }

    private async Task SignInUser(FortUser user)
    {
        try
        {
            logger.LogDebug("Automatically signing the user in");

            // Sign in with a cookie
            await signInManager.SignInAsync(user, isPersistent: false).ConfigureAwait(false);

            logger?.LogInformation("The new user was automatically signed");
            navigationManager.NavigateToLocal(ReturnUrl ?? GeneralPages.Home.Url);
        }
        catch (NavigationException)
        {
            // We ignore this with a SSR component, but MUST let it happen
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error automatically signing the user in");
        }
    }
}

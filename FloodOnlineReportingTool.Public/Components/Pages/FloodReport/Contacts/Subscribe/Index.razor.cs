using FloodOnlineReportingTool.Database.Models.Contact;
using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.Public.Models;
using FloodOnlineReportingTool.Public.Models.FloodReport.Contact;
using FloodOnlineReportingTool.Public.Models.Order;
using FloodOnlineReportingTool.Public.Services;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.Extensions.Logging;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport.Contacts.Subscribe;

public partial class Index(
    ILogger<Index> logger,
    IContactRecordRepository contactRepository,
    IGovNotifyEmailSender govNotifyEmailSender,
    NavigationManager navigationManager,
    SessionStateService scopedSessionStorage,
    IGdsJsInterop gdsJs
) : IPageOrder, IAsyncDisposable
{
    private readonly CancellationTokenSource _cts = new();
    private Guid _verificationId = Guid.Empty;
    private bool _isLoading = true;

    [SupplyParameterFromQuery]
    private bool FromSummary { get; set; }

    private EditContext _editContext = default!;


    public required ContactSubscriptionRecord _contactModel { get; set; } = default!;

    // Page order properties
    public string Title { get; set; } = SubscriptionPages.Home.Title;
    public IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [
        GeneralPages.Home.ToGdsBreadcrumb(),
        FloodReportPages.Overview.ToGdsBreadcrumb(),
    ];

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

    protected override async Task OnInitializedAsync()
    {
        // Setup model and edit context
        if (_contactModel == null)
        {
            _contactModel = new();
            _editContext = new(_contactModel);
            _editContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _verificationId = await scopedSessionStorage.GetVerificationId();

            _isLoading = false;
            StateHasChanged();
            await gdsJs.InitGds(_cts.Token);
        }
    }

    private async Task OnSubmit()
    {

        if (!_editContext.Validate())
        {
            return;
        }

        ContactSubscriptionCreateResult subscriptionResult = await CreateSubscription();

        if (!subscriptionResult.IsSuccess)
        {
            return;
        }

        // Success - send confirmation email
        // Lucy to enable the code below
        //var sentNotification = await govNotifyEmailSender.SendEmailVerificationNotification(
        //    subscriptionResult.ContactSubscriptionRecord!.EmailAddress,
        //    subscriptionResult.ContactSubscriptionRecord!.ContactName,
        //    subscriptionResult.ContactSubscriptionRecord!.VerificationCode,
        //    subscriptionResult.ContactSubscriptionRecord!.VerificationExpiryUtc
        //    );

        await scopedSessionStorage.SaveVerificationId(subscriptionResult.ContactSubscriptionRecord!.Id);

        var nextPageUrl = SubscriptionPages.Verify.Url;
        if (FromSummary)
        {
            nextPageUrl = SubscriptionPages.Summary.Url;
        }
        navigationManager.NavigateTo(nextPageUrl);
    }

    private async Task<ContactSubscriptionCreateResult> CreateSubscription()
    {
        logger.LogDebug("Creating subscription information");

        return await contactRepository.CreateSubscriptionRecord(_contactModel, _cts.Token);

    }
}
using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.Public.Models;
using FloodOnlineReportingTool.Public.Models.FloodReport.Contact;
using FloodOnlineReportingTool.Public.Models.FloodReport.Contact.Subscribe;
using FloodOnlineReportingTool.Public.Models.Order;
using FloodOnlineReportingTool.Public.Services;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Security.Cryptography;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport.Contacts.Subscribe;

public partial class Verify(
    ILogger<Verify> logger,
    IContactRecordRepository contactRepository,
    IGovNotifyEmailSender govNotifyEmailSender,
    SessionStateService scopedSessionStorage,
    NavigationManager navigationManager,
    IGdsJsInterop gdsJs
) : IPageOrder, IAsyncDisposable
{
    private readonly CancellationTokenSource _cts = new();
    private Guid _verificationId = Guid.Empty;
    private bool _isLoading = true;
    private bool? isResent;

    // Page order properties
    public string Title { get; set; } = SubscriptionPages.Verify.Title;
    public IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [
        GeneralPages.Home.ToGdsBreadcrumb(),
        FloodReportPages.Overview.ToGdsBreadcrumb(),
    ];

    private EditContext _editContext = default!;

    private VerifyModel Model { get; set; } = default!;

    private ValidationMessageStore _messageStore = default!;

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
            _verificationId = await scopedSessionStorage.GetVerificationId();

            var subscribeRecord = await contactRepository.GetSubscriptionRecordById(_verificationId, _cts.Token);
            if (subscribeRecord == null)
            {
                logger.LogWarning("No subscription record found for verification ID {VerificationId}", _verificationId);
                // Handle missing subscription record as needed
                _isLoading = false;
                StateHasChanged();
                return;
            }

            Model = new VerifyModel
            {
                Id = subscribeRecord.Id,
                ContactName = subscribeRecord.ContactName,
                EmailAddress = subscribeRecord.EmailAddress,
                IsEmailVerified = subscribeRecord.IsEmailVerified
            };

            // Recreate the EditContext with the new Model instance
            _editContext = new(Model);
            _editContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
            _messageStore = new(_editContext);

            _isLoading = false;
            StateHasChanged();
            await gdsJs.InitGds(_cts.Token);
        }
    }

    private async Task OnValidSubmit()
    {
        _messageStore.Clear();

        bool VerifiedResult = await contactRepository.VerifySubscriptionRecord(Model.Id, (int)Model.EnteredCodeNumber, _cts.Token);

        if (!VerifiedResult)
        {
            _messageStore.Add(_editContext.Field(nameof(Model.EnteredCodeNumber)),
            "The code you provided was not correct. Please try again or request a new code.");
            _editContext.NotifyValidationStateChanged();
            return;
        }

        navigationManager.NavigateTo(SubscriptionPages.Summary.Url);
    }

    private async Task OnResend()
    {
        isResent = false;

        var VerificationCode = RandomNumberGenerator.GetInt32(100000, 1000000);
        var VerificationExpiryUtc = DateTimeOffset.UtcNow.AddMinutes(30);

        var sentNotification = await govNotifyEmailSender.SendEmailVerificationNotification(
            Model.EmailAddress,
            Model.ContactName,
            VerificationCode,
            VerificationExpiryUtc
            );
        bool sent = false;

        if (sent)
        {
            isResent = true;
        }
        StateHasChanged();
    }
}
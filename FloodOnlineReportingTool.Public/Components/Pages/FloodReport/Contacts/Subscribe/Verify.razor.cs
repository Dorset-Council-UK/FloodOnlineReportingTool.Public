using FloodOnlineReportingTool.Contracts.Shared;
using FloodOnlineReportingTool.Database.Models.Contact;
using FloodOnlineReportingTool.Database.Models.Contact.Subscribe;
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
    private Guid _floodReportId = Guid.Empty;
    private SubscribeRecord? _subscribeRecord;
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

            _subscribeRecord = await contactRepository.GetSubscriptionRecordById(_verificationId, _cts.Token);
            if (_subscribeRecord == null)
            {
                logger.LogWarning("No subscription record found for verification ID {VerificationId}", _verificationId);
                // Handle missing subscription record as needed
                _isLoading = false;
                StateHasChanged();
                return;
            }

            Model = new VerifyModel
            {
                Id = _subscribeRecord.Id,
                ContactName = _subscribeRecord.ContactName,
                EmailAddress = _subscribeRecord.EmailAddress,
                IsEmailVerified = _subscribeRecord.IsEmailVerified,
                ContactType = _subscribeRecord.ContactType,
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

    private async Task OnSubmit()
    {
        _messageStore.Clear();

        if (!_editContext.Validate())
        {
            StateHasChanged();
            return;
        }
        if (Model.EnteredCodeNumber is not int enteredCode)
        {
            StateHasChanged();
            return;
        }

        bool VerifiedResult = await contactRepository.VerifySubscriptionRecord(Model.Id, enteredCode, _cts.Token);

        if (!VerifiedResult)
        {
            CustomLogError(nameof(Model.EnteredCodeNumber),"Incorrect verification code entered.", "The code you provided was not correct. Please try again or request a new code.", false);
            _editContext.NotifyValidationStateChanged();
            return;
        }

        // Generate a contact record
        _floodReportId = await scopedSessionStorage.GetFloodReportId();
        ContactRecordDto dto = new ContactRecordDto
        {
            ContactName = Model.ContactName,
            EmailAddress = Model.EmailAddress,
            IsEmailVerified = true,
            SubscribeRecord = _subscribeRecord,
            ContactType = Model.ContactType,

        };
        var contactRecord = await contactRepository.CreateForReport(_floodReportId, dto, _cts.Token);

        navigationManager.NavigateTo(ContactPages.Summary.Url);
    }

    private async Task OnResend()
    {
        isResent = false;

        var subscribeRecord = await contactRepository.GetSubscriptionRecordById(_verificationId, _cts.Token);
        if ( subscribeRecord == null)
        {
            StateHasChanged();
            return;
        }
        var updatedSubscription = await contactRepository.UpdateVerificationCode(subscribeRecord, _cts.Token);
        if (updatedSubscription.ContactSubscriptionRecord is not SubscribeRecord returnedSubscription)
        {
            StateHasChanged();
            return;
        }
        if (returnedSubscription.VerificationExpiryUtc is not DateTimeOffset expiry)
        {
            StateHasChanged();
            return;
        }

        try
        {
            logger.LogInformation("Sending email verification notification");
            var sentNotification = await govNotifyEmailSender.SendEmailVerificationNotification(
                returnedSubscription.EmailAddress,
                returnedSubscription.ContactName,
                returnedSubscription.VerificationCode,
                expiry
                );
            isResent = true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending email verification notification: {ErrorMessage}", ex.Message);
        }

        StateHasChanged();
        return;
    }

    private void CustomLogError(string fieldname, string errorMessage, string returnMessage, bool logMessage)
    {
        if (logMessage)
        {
            logger.LogWarning(errorMessage);
        }
        _messageStore.Add(_editContext.Field(fieldname), returnMessage);
        _editContext.NotifyValidationStateChanged();
    }
}
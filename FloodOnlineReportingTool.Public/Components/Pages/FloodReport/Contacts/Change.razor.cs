using FloodOnlineReportingTool.Contracts.Shared;
using FloodOnlineReportingTool.Database.Models;
using FloodOnlineReportingTool.Database.Models.Contact.Subscribe;
using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.Public.Models.FloodReport.Contact;
using FloodOnlineReportingTool.Public.Models.Order;
using FloodOnlineReportingTool.Public.Services;
using FloodOnlineReportingTool.Public.Validators.Contacts;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport.Contacts;

public partial class Change(
    ILogger<Change> logger,
    NavigationManager navigationManager,
    IContactRecordRepository contactRepository,
    IFloodReportRepository floodReportRepository,
    SessionStateService scopedSessionStorage,
    IGovNotifyEmailSender govNotifyEmailSender
) : IPageOrder, IAsyncDisposable
{
    // Page order properties
    public string Title { get; set; } = ContactPages.Change.Title;
    public IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [
        GeneralPages.Home.ToGdsBreadcrumb(),
        FloodReportPages.Overview.ToGdsBreadcrumb(),
        ContactPages.Summary.ToGdsBreadcrumb(),
    ];

    [Parameter]
    public Guid ContactId { get; set; }

    [CascadingParameter]
    public Task<AuthenticationState>? AuthenticationState { get; set; }

    [CascadingParameter]
    public EditContext EditContext { get; set; } = default!;
    private EditContext _editContext = default!;

    public IReadOnlyCollection<GdsOptionItem<ContactRecordType>> ContactTypes = [];
    private ContactModel? _contactModel;

    private SubscribeRecord? _subscribeModel;
    private Guid _floodReportId = Guid.Empty;
    private Guid _userId = Guid.Empty;
    private string _floodReportReference = string.Empty;
    private Database.Models.Flood.FloodReport? _floodReport;
    private bool _isLoading = true;
    private bool _isDataLoading = true;
    private bool isResent = false;
    private ValidationMessageStore _messageStore = default!;
    private readonly CancellationTokenSource _cts = new();

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
            _messageStore = new(_editContext);
        }

        // Check if user is authenticated
        if (AuthenticationState is not null)
        {

            var authState = await AuthenticationState;
            var user = authState.User;

            var oidClaim = user.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;
            _userId = Guid.TryParse(oidClaim, out var parsedOid) ? parsedOid : Guid.Empty;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _floodReportId = await scopedSessionStorage.GetFloodReportId();

            _subscribeModel = await contactRepository.GetSubscriptionRecordById(ContactId, _cts.Token);
            if (_subscribeModel is not null)
            {
                // Update the existing _contactModel properties instead of replacing the object
                _contactModel!.EmailAddress = _subscribeModel!.EmailAddress;
                _contactModel.IsEmailVerified = _subscribeModel.IsEmailVerified;
                _contactModel.ContactName = _subscribeModel.ContactName;
                _contactModel.ContactType = _subscribeModel.ContactType;
                _contactModel.Id = _subscribeModel.Id;
                _contactModel.IsRecordOwner = _subscribeModel.IsRecordOwner;
                _contactModel.PhoneNumber = _subscribeModel.PhoneNumber;
                _contactModel.ContactUserId = _subscribeModel.ContactRecordId;

            }

            var allUnsedTypes = await contactRepository.GetUnusedRecordTypes(_floodReportId, _cts.Token);
            var allTypes = Enum.GetValues<ContactRecordType>();
            List<ContactRecordType> availableTypes = [];
            foreach (var t in allTypes)
            {
                if (allUnsedTypes.Contains(t) || t == _contactModel!.ContactType)
                {
                    availableTypes.Add(t);
                }
            }
            ContactTypes = [.. availableTypes.Select(CreateOption)];

            _isDataLoading = false;
            _isLoading = false;
            StateHasChanged(); 
        }
        
    }

    private async Task OnSubmit()
    {
        _messageStore.Clear();

        // Manual FluentValidation - only runs on submit
        var validator = new ContactModelValidator();
        var validationResult = await validator.ValidateAsync(_contactModel, _cts.Token);

        if (!validationResult.IsValid)
        {
            // Add FluentValidation errors to EditContext
            foreach (var error in validationResult.Errors)
            {
                var fieldIdentifier = _editContext.Field(error.PropertyName);
                _messageStore.Add(fieldIdentifier, error.ErrorMessage);
            }

            _editContext.NotifyValidationStateChanged();
            StateHasChanged();
            return;
        }

        await UpdateContact();
    }

    private async Task UpdateContact()
    {
        logger.LogDebug("Updating contact information");
        
        if (_contactModel == null)
        {
            return;
        }

        try
        {

            var selectedRecord = await contactRepository.GetSubscriptionRecordById(ContactId, _cts.Token);
            if (selectedRecord is null)
            {
                return;
            }

            selectedRecord.PhoneNumber = _contactModel.PhoneNumber;
            selectedRecord.EmailAddress = _contactModel.EmailAddress!;
            selectedRecord.ContactName = _contactModel.ContactName!;
            selectedRecord.ContactType = _contactModel.ContactType!.Value;

            var updatedSubscription = await contactRepository.UpdateSubscriptionRecord(selectedRecord, _cts.Token);

            logger.LogInformation("Contact information updated successfully for user {UserId}", _userId);

            if (updatedSubscription.SubscriptionRecord is not SubscribeRecord sub)
            {
                // Something when wrong!
                return;
            }
            if (!sub.IsEmailVerified)
            {
                var updatedVerification = await contactRepository.UpdateVerificationCode(sub, false, _cts.Token);
                if (updatedVerification.SubscriptionRecord is not SubscribeRecord returnedSubscription)
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
                    // TODO: fix this, how do we create and send link emails?
                    var sentNotification = await govNotifyEmailSender.SendEmailVerificationLinkNotification(
                        returnedSubscription.EmailAddress,
                        returnedSubscription.ContactName,
                        "Unknown",
                        "To fix",
                        expiry
                        );
                    isResent = true;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error sending email verification notification: {ErrorMessage}", ex.Message);
                }
            }

            // Navigate back to contacts home
            navigationManager.NavigateTo(ContactPages.Summary.Url);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "There was a problem updating contact information");
            _messageStore.Add(_editContext.Field(nameof(_contactModel.ContactType)), $"There was a problem updating the contact information. Please try again but if this issue happens again then please report a bug.");
            _editContext.NotifyValidationStateChanged();
        }
    }

    private GdsOptionItem<ContactRecordType> CreateOption(ContactRecordType contactRecordType)
    {
        var id = contactRecordType.ToString().AsSpan();
        var selected = false;
        return new GdsOptionItem<ContactRecordType>(id, contactRecordType.LabelText(), contactRecordType, selected, hint: contactRecordType.HintText());
    }
}

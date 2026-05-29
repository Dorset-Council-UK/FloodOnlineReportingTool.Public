using FloodOnlineReportingTool.Contracts.Shared;
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
using System.Security.Claims;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport.Contacts;

public partial class Create(
    ILogger<Create> logger,
    NavigationManager navigationManager,
    IContactRecordRepository contactRepository,
    ISubscribeRecordRepository subscribeRecordRepository,
    SessionStateService scopedSessionStorage,
    IGovNotifyEmailSender govNotifyEmailSender
) : IPageOrder, IAsyncDisposable
{
    // Public Properties
    public string Title { get; set; } = ContactPages.Create.Title;
    public IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [
        GeneralPages.Home.ToGdsBreadcrumb(),
        FloodReportPages.Overview.ToGdsBreadcrumb(),
        ContactPages.Summary.ToGdsBreadcrumb(),
    ];

    // Parameters
    [CascadingParameter]
    public Task<AuthenticationState>? AuthenticationState { get; set; }

    [CascadingParameter]
    public EditContext EditContext { get; set; } = default!;

    // Private Fields
    private EditContext _editContext = default!;
    private ValidationMessageStore _messageStore = default!;
    private ContactModel? _contactModel;
    private Database.Models.Flood.FloodReport? _floodReport;
    private Guid _floodReportId;
    private string? _userId;
    private bool _isLoading = true;
    private readonly CancellationTokenSource _cts = new();

    // Public Properties
    public IReadOnlyCollection<GdsOptionItem<ContactRecordType>> ContactTypes = [];

    public async ValueTask DisposeAsync()
    {
        try
        {
            await _cts.CancelAsync();
            _cts.Dispose();
        }
        catch (Exception)
        {
            // Suppressing exception during disposal to prevent issues during component teardown
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
            ContactTypes = CreateContactTypeOptions();
        }

        // Check if user is authenticated
        if (AuthenticationState is not null)
        {

            var authState = await AuthenticationState;
            var user = authState.User;
            _userId = user.Oid;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {           
            _floodReportId = await scopedSessionStorage.GetFloodReportId();
            var reportOwnerSubscribeRecord = await subscribeRecordRepository.GetReportOwnerContactByReport(_floodReportId, _cts.Token);
            if (reportOwnerSubscribeRecord is null)
            {
                // This is not allowed, setup an owner
                navigationManager.NavigateTo($"{SubscriptionPages.Home.Url}?Owns=true");
                return;
            }

            // This is a create page, so we need to get the unused contact types only
            var allTypes = await contactRepository.GetUnusedRecordTypes(_floodReportId, _cts.Token);
            ContactTypes = [.. allTypes.Select(CreateOption)];

            _isLoading = false;
            StateHasChanged();
            
        }
    }

    // Private Methods

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

        await CreateContact();
    }

    private async Task CreateContact()
    {
        logger.LogDebug("Creating contact information");
        try
        {
            if (_contactModel == null)
            {
                logger.LogError("There was a problem creating contact information");
                return;
            }

            // Generate a contact record
            _floodReportId = await scopedSessionStorage.GetFloodReportId();
            SubscribeRecordDto subscribeRecordDto = new()
            {
                ContactType = _contactModel.ContactType!.Value,
                ContactName = _contactModel.ContactName!,
                EmailAddress = _contactModel.EmailAddress!,
                PhoneNumber = _contactModel.PhoneNumber,
                IsRecordOwner = false,
            };

            Guid? contactRecordId = null;
            var contactRecords = await contactRepository.GetContactsByReport(_floodReportId, _cts.Token);
            if (contactRecords.Count == 0)
            {
                var createResult = await contactRepository.Create(_userId, _floodReportId, _cts.Token);
                if (!createResult.IsSuccess)
                {
                    foreach (var error in createResult.Errors)
                    {
                        logger.LogError("Error creating contact record: {ErrorMessage}", error);
                    }
                    return;
                }
                contactRecordId = createResult.Value.Id;
            }
            else
            {
                contactRecordId = contactRecords.FirstOrDefault()?.Id;
                if (contactRecordId is null)
                {
                    return;
                }
            }

            // Get authentication state
            var currentUserEmail = string.Empty;
            if (AuthenticationState is not null)
            {
                var authState = await AuthenticationState;
                currentUserEmail = authState.User.Email;
            }
            var generatedSubscribeRecord = await subscribeRecordRepository.Create(contactRecordId.Value, subscribeRecordDto, currentUserEmail, userPresent: false, _cts.Token);
            if (!generatedSubscribeRecord.IsSuccess)
            {
                logger.LogError("There was a problem creating contact information");
                return;
            }

            // TODO: Why create a new subscribe record and then update the verification code immediately?

            SubscribeRecord subscribeRecord = generatedSubscribeRecord.Value;
            if (!subscribeRecord.IsEmailVerified && !subscribeRecord.IsRecordOwner)
            {
                var updateResult = await subscribeRecordRepository.UpdateVerificationCode(subscribeRecord, userPresent: false, _cts.Token);
                if (!updateResult.IsSuccess)
                {
                    foreach (var error in updateResult.Errors)
                    {
                        logger.LogError("Error updating verification code: {ErrorMessage}", error);
                    }
                    navigationManager.NavigateTo(ContactPages.Summary.Url);
                    return;
                }

                SubscribeRecord returnedSubscription = updateResult.Value;
                if (returnedSubscription.VerificationExpiryUtc is not DateTimeOffset expiry)
                {
                    logger.LogError("Error sending email verification notification");
                    navigationManager.NavigateTo(ContactPages.Summary.Url);
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
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error sending email verification notification: {ErrorMessage}", ex.Message);
                }
            }
        
            logger.LogInformation("Contact information created successfully for report {_floodReportId}", _floodReportId);
            navigationManager.NavigateTo(ContactPages.Summary.Url);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "There was a problem creating contact information");
            _messageStore.Add(_editContext.Field(nameof(_contactModel.ContactType)), $"There was a problem creating the contact information. Please try again but if this issue happens again then please report a bug.");
            _editContext.NotifyValidationStateChanged();
        }
    }

    private IReadOnlyCollection<GdsOptionItem<ContactRecordType>> CreateContactTypeOptions()
    {

        var allTypes = Enum.GetValues<ContactRecordType>();

        return [.. allTypes.Select(CreateOption)];
    }

    /// <summary>
    /// Creates a GDS option item for a contact record type.
    /// </summary>
    private GdsOptionItem<ContactRecordType> CreateOption(ContactRecordType contactRecordType)
    {
        var id = contactRecordType.ToString().AsSpan();
        var selected = false;
        return new GdsOptionItem<ContactRecordType>(id, contactRecordType.LabelText(), contactRecordType, selected, hint: contactRecordType.HintText());
    }
}

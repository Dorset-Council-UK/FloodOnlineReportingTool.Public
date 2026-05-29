using FloodOnlineReportingTool.Contracts.Shared;
using FloodOnlineReportingTool.Database.Models.Contact.Subscribe;
using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.Public.Models.FloodReport.Contact.Subscribe;
using FloodOnlineReportingTool.Public.Models.Order;
using FloodOnlineReportingTool.Public.Services;
using FloodOnlineReportingTool.Public.Validators.Contacts;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using System.Security.Claims;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport.Contacts.Subscribe;

public partial class Index(
    ILogger<Index> logger,
    IContactRecordRepository contactRepository,
    ISubscribeRecordRepository subscribeRecordRepository,
    IGovNotifyEmailSender govNotifyEmailSender,
    NavigationManager navigationManager,
    SessionStateService scopedSessionStorage
) : IPageOrder, IAsyncDisposable
{
    // Parameters
    [SupplyParameterFromQuery]
    private bool Me { get; set; }

    [SupplyParameterFromQuery]
    private bool Owns { get; set; }

    [SupplyParameterFromQuery]
    private bool FromSummary { get; set; }

    [CascadingParameter]
    public Task<AuthenticationState>? AuthenticationState { get; set; }

    public required SubscribeModel Model { get; set; } = default!;

    // Private Fields
    private readonly CancellationTokenSource _cts = new();
    private Guid _verificationId = Guid.Empty;
    private Guid _floodReportId = Guid.Empty;
    private string? _userID;
    private bool _isLoading = true;
    private EditContext _editContext = default!;
    private ValidationMessageStore _messageStore = default!;

    // Public Properties
    public IReadOnlyCollection<GdsOptionItem<ContactRecordType>> ContactTypes = [];
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
        if (Model == null)
        {
            Model = new()
            {
                ContactRecord = new(),
            };
            _editContext = new(Model);
            _editContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
            _messageStore = new(_editContext);
            ContactTypes = CreateContactTypeOptions();
        }

        // Check if user is authenticated
        if (AuthenticationState is not null)
        {
            var authState = await AuthenticationState;
            _userID = authState.User.Oid;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _verificationId = await scopedSessionStorage.GetVerificationId();

            if (Me)
            {
                if (string.IsNullOrEmpty(_userID))
                {
                    // Can't proceed if not authenticated
                    navigationManager.NavigateTo(ContactPages.Summary.Url);
                    return;
                }

                // Get authentication state
                var currentUserEmail = string.Empty;
                var currentUserDisplayName = string.Empty;
                if (AuthenticationState is not null)
                {
                    var authState = await AuthenticationState;
                    currentUserEmail = authState.User.Email;
                    currentUserDisplayName = authState.User.DisplayName;
                }

                // Check if this user already has a contact record
                var contactRecord = await contactRepository.Get(_userID, _cts.Token);
                if (contactRecord != null)
                {
                    // Connect to the existing record and skip the subscription setup steps
                    _floodReportId = await scopedSessionStorage.GetFloodReportId();

                    var updateContactRecord = await contactRepository.LinkContactByReport(_floodReportId, contactRecord.Id, _cts.Token);
                    if (updateContactRecord.IsSuccess)
                    {
                        var recordOwner = await subscribeRecordRepository.GetReportOwnerContactByReport(_floodReportId, _cts.Token);
                        if (recordOwner == null)
                        {
                            // Can't proceed if not authenticated
                            navigationManager.NavigateTo(ContactPages.Summary.Url);
                            return;
                        }

                        // Store session info
                        await scopedSessionStorage.SaveVerificationId(recordOwner.Id);

                        // Proceed to summary
                        var nextPageUrl = ContactPages.Summary.Url;
                        navigationManager.NavigateTo(nextPageUrl);
                        return;
                    }
                }

                // Pre-fill email if known
                if (string.IsNullOrWhiteSpace(Model.ContactName))
                {
                    Model.ContactName = currentUserDisplayName;
                }

                if (string.IsNullOrWhiteSpace(Model.EmailAddress))
                {
                    Model.EmailAddress = currentUserEmail;
                }

                // Notify EditContext of changes
                if (!string.IsNullOrWhiteSpace(Model.ContactName))
                {
                    _editContext.NotifyFieldChanged(_editContext.Field(nameof(Model.ContactName)));
                }
                if (!string.IsNullOrWhiteSpace(Model.EmailAddress))
                {
                    _editContext.NotifyFieldChanged(_editContext.Field(nameof(Model.EmailAddress)));
                }
            }

            _isLoading = false;
            StateHasChanged();
        }
    }

    private async Task OnSubmit()
    {
        _messageStore.Clear();

        // Manual FluentValidation - only runs on submit
        var validator = new IndexValidator();
        var validationResult = await validator.ValidateAsync(Model, _cts.Token);

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

        // Error handling back into the Razor. Use validation message store or ErrorBoundary

        // Generate a contact record
        _floodReportId = await scopedSessionStorage.GetFloodReportId();
        SubscribeRecordDto subscribeRecordDto = new()
        {
            ContactType = Model.ContactType,
            ContactName = Model.ContactName,
            EmailAddress = Model.EmailAddress,
            IsRecordOwner = Owns,
        };

        Guid? contactRecordId;
        var contactRecords = await contactRepository.GetContactsByReport(_floodReportId, _cts.Token);
        if (contactRecords.Count == 0)
        {
            var createResult = await contactRepository.Create(_userID, _floodReportId, _cts.Token);
            if (!createResult.IsSuccess)
            {
                foreach (var error in createResult.Errors)
                {
                    logger.LogWarning("Couldn't create a subscription record: {ErrorMessage}", error);
                    _messageStore.Add(_editContext.Field(nameof(Model.ErrorMessage)), "Sorry, something went wrong");
                }
                _editContext.NotifyValidationStateChanged();
                return;
            }
            contactRecordId = createResult.Value.Id;
        }
        else
        {
            contactRecordId = contactRecords.FirstOrDefault()?.Id;
            if (contactRecordId is null)
            {
                logger.LogWarning("Couldn't find a contact record for this flood report.");
                _messageStore.Add(_editContext.Field(nameof(Model.ErrorMessage)), "Sorry, something went wrong");
                _editContext.NotifyValidationStateChanged();
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

        var createSubscribeRecord = await subscribeRecordRepository.Create(contactRecordId.Value, subscribeRecordDto, currentUserEmail, userPresent: true, _cts.Token);
        if (!createSubscribeRecord.IsSuccess)
        {
            logger.LogWarning("Created subscription record not returned.");
            _messageStore.Add(_editContext.Field(nameof(Model.ErrorMessage)), "Sorry, something went wrong");
            _editContext.NotifyValidationStateChanged();
            return;
        }

        SubscribeRecord returnedSubscription = createSubscribeRecord.Value;
        if (returnedSubscription.VerificationExpiryUtc is not DateTimeOffset expiry)
        {
            logger.LogWarning("Subscription record verification expiry is not valid.");
            _messageStore.Add(_editContext.Field(nameof(Model.ErrorMessage)), "Sorry, something went wrong");
            _editContext.NotifyValidationStateChanged();
            return;
        }

        // Success
        // We are past the point of no return - all errors from here need to be passed to the next page
        // Use try catch and pass errors via query string or session storage if needed

        // Store session info
        await scopedSessionStorage.SaveVerificationId(returnedSubscription.Id);

        if (returnedSubscription.IsEmailVerified)
        {
            // Send them onwards
            var nextPageUrl = ContactPages.Summary.Url;
            navigationManager.NavigateTo(nextPageUrl);
        }
        else
        {
            // send confirmation email
            try
            {
                logger.LogInformation("Sending email verification notification");
                var sentNotification = await govNotifyEmailSender.SendEmailVerificationNotification(
                    returnedSubscription.EmailAddress,
                    returnedSubscription.ContactName,
                    returnedSubscription.VerificationCode,
                    expiry
                );
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error sending email verification notification: {ErrorMessage}", ex.Message);
            }

            // Send them onwards
            var nextPage = FromSummary ? ContactPages.Summary : SubscriptionPages.Verify;
            // TODO: pass that the email sending failed if emailSent is false
            navigationManager.NavigateTo(nextPage.Url);
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
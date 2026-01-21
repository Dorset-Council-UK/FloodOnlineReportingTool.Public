using FloodOnlineReportingTool.Contracts.Shared;
using FloodOnlineReportingTool.Database.Models.Contact;
using FloodOnlineReportingTool.Database.Models.Contact.Subscribe;
using FloodOnlineReportingTool.Database.Models.ResultModels;
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
    private Guid _userID = Guid.Empty;
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
            Model = new();
            Model.ContactRecord = new();
            _editContext = new(Model);
            _editContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
            _messageStore = new(_editContext);
            ContactTypes = CreateContactTypeOptions();
        }

        // Check if user is authenticated
        if (AuthenticationState is not null)
        {
            var authState = await AuthenticationState;
            _userID = authState.User.UserOid();
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _verificationId = await scopedSessionStorage.GetVerificationId();

            if (Me)
            {
                if (_userID == Guid.Empty)
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
                    currentUserEmail = authState.User.Email();
                    currentUserDisplayName = authState.User.DisplayName();
                }

                // Check if this user already has a contact record
                var contactRecord = await contactRepository.ContactRecordExistsForUser(_userID, _cts.Token);
                if (contactRecord != Guid.Empty)
                {
                    var contactRecordId = contactRecord.Value;
                    // Connect to the existing record and skip the subscription setup steps
                    _floodReportId = await scopedSessionStorage.GetFloodReportId();

                    var linkResult = await contactRepository.LinkContactByReport(_floodReportId, contactRecordId, _cts.Token);
                    if (linkResult.IsSuccess)
                    {
                        var recordOwner = await contactRepository.GetReportOwnerContactByReport(_floodReportId, _cts.Token);
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
        ContactRecordDto dto = new ContactRecordDto
        {
            UserId = _userID == Guid.Empty ? null : _userID,
            ContactType = Model.ContactType,
            ContactName = Model.ContactName!,
            EmailAddress = Model.EmailAddress!,
            IsRecordOwner = Owns
        };
        var contactRecord = await contactRepository.GetContactsByReport(_floodReportId, _cts.Token);
        Guid contactRecordId;
        if (contactRecord.Count == 0)
        {
            var newRecord = await contactRepository.CreateForReport(_floodReportId, dto, _cts.Token);
            if (!newRecord.IsSuccess)
            {
                CustomLogError(nameof(Model.ErrorMessage), "Couldn't create a subscription record.", "Sorry, something went wrong", true);
                return;
            }
            contactRecordId = newRecord.ResultModel!.Id;
        }
        else
        {
            contactRecordId = contactRecord.First().Id;
        }

        // Get authentication state
        var currentUserEmail = string.Empty;
        if (AuthenticationState is not null)
        {
            var authState = await AuthenticationState;
            currentUserEmail = authState.User.Email();
        }

        CreateOrUpdateResult<SubscribeRecord> subscriptionResult = await contactRepository.CreateSubscriptionRecord(contactRecordId, dto, currentUserEmail, true, _cts.Token);

        if (subscriptionResult.ResultModel is not SubscribeRecord returnedSubscription)
        {
            CustomLogError(nameof(Model.ErrorMessage), "Created subscription record not returned.", "Sorry, something went wrong", true);
            return;
        }
        if (returnedSubscription.VerificationExpiryUtc is not DateTimeOffset expiry)
        {
            CustomLogError(nameof(Model.ErrorMessage), "Subscription record verification expiry is not valid.", "Sorry, something went wrong", true);
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
            var nextPageUrl = SubscriptionPages.Verify.Url;
            if (FromSummary)
            {
                nextPageUrl = ContactPages.Summary.Url;
            }
            var NavigationOptions = new NavigationOptions
            {

            };
            // TODO: pass that the email sending failed if emailSent is false
            navigationManager.NavigateTo(nextPageUrl);
        }
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
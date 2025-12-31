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

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport.Contacts;

/// <summary>
/// Page component for changing contact information associated with a flood report.
/// Handles both authenticated and unauthenticated scenarios with different validation approaches.
/// </summary>
/// <remarks>
/// Design decision: This component uses manual FluentValidation on submit for unauthenticated users
/// to avoid validation glitches when the form is partially filled. Authenticated users get real-time validation.
/// Email verification is automatically triggered when an unverified email address is updated.
/// </remarks>
public partial class Change(
    ILogger<Change> logger,
    NavigationManager navigationManager,
    IContactRecordRepository contactRepository,
    SessionStateService scopedSessionStorage,
    IGovNotifyEmailSender govNotifyEmailSender
) : IPageOrder, IAsyncDisposable
{
    #region Page Properties

    /// <summary>
    /// Gets or sets the page title displayed in the browser and page header.
    /// </summary>
    public string Title { get; set; } = ContactPages.Change.Title;

    /// <summary>
    /// Gets or sets the breadcrumb navigation items for the page.
    /// </summary>
    public IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } =
    [
        GeneralPages.Home.ToGdsBreadcrumb(),
        FloodReportPages.Overview.ToGdsBreadcrumb(),
        ContactPages.Summary.ToGdsBreadcrumb(),
    ];

    #endregion

    #region Parameters

    /// <summary>
    /// Gets or sets the unique identifier of the contact record to change.
    /// </summary>
    [Parameter]
    public Guid ContactId { get; set; }

    /// <summary>
    /// Gets or sets the cascading authentication state parameter.
    /// Used to determine if the user is authenticated and retrieve their user ID.
    /// </summary>
    [CascadingParameter]
    public Task<AuthenticationState>? AuthenticationState { get; set; }

    /// <summary>
    /// Gets or sets the cascading edit context parameter.
    /// Note: This is overridden by the local _editContext field.
    /// </summary>
    [CascadingParameter]
    public EditContext EditContext { get; set; } = default!;

    #endregion

    #region Private Fields

    /// <summary>
    /// The edit context used for form validation and state management.
    /// </summary>
    private EditContext _editContext = default!;

    /// <summary>
    /// The validation message store for managing validation errors.
    /// </summary>
    private ValidationMessageStore _messageStore = default!;

    /// <summary>
    /// The contact model bound to the form for editing.
    /// </summary>
    private ContactModel? _contactModel;

    /// <summary>
    /// The subscription record retrieved from the database.
    /// Used to populate the contact model and track verification status.
    /// </summary>
    private SubscribeRecord? _subscribeModel;

    /// <summary>
    /// The ID of the flood report associated with this contact.
    /// Retrieved from session storage.
    /// </summary>
    private Guid _floodReportId = Guid.Empty;

    /// <summary>
    /// The authenticated user's ID from the Entra ID (formerly Azure AD) object identifier claim.
    /// </summary>
    private Guid _userId = Guid.Empty;

    /// <summary>
    /// Indicates whether the page is in a loading state (spinner visible).
    /// </summary>
    private bool _isLoading = true;

    /// <summary>
    /// Indicates whether data is currently being loaded from the database.
    /// </summary>
    private bool _isDataLoading = true;

    /// <summary>
    /// Indicates whether a verification email has been resent to the user.
    /// </summary>
    private bool _isResent;

    /// <summary>
    /// Cancellation token source for managing async operation cancellation during disposal.
    /// </summary>
    private readonly CancellationTokenSource _cts = new();

    #endregion

    #region Public Properties

    /// <summary>
    /// Gets the available contact types that can be selected.
    /// Excludes types already in use for the current flood report, except the current contact's type.
    /// </summary>
    public IReadOnlyCollection<GdsOptionItem<ContactRecordType>> ContactTypes { get; private set; } = [];

    #endregion

    #region Lifecycle Methods

    /// <summary>
    /// Disposes of managed resources asynchronously.
    /// Cancels any pending operations and releases the cancellation token source.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous dispose operation.</returns>
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

    /// <summary>
    /// Initializes the component on first load.
    /// Sets up the contact model, edit context, validation message store, and retrieves the authenticated user's ID.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    protected override async Task OnInitializedAsync()
    {
        // Setup model and edit context
        _contactModel ??= new ContactModel();
        _editContext = new EditContext(_contactModel);
        _editContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
        _messageStore = new ValidationMessageStore(_editContext);

        // Check if user is authenticated and retrieve their user ID from Entra ID claims
        if (AuthenticationState is not null)
        {
            var authState = await AuthenticationState;
            var user = authState.User;

            // Extract the Entra ID (formerly Azure AD) object identifier claim
            var oidClaim = user.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;
            _userId = Guid.TryParse(oidClaim, out var parsedOid) ? parsedOid : Guid.Empty;
        }
    }

    /// <summary>
    /// Loads contact data after the first render.
    /// Retrieves the flood report ID, subscription record, and available contact types.
    /// </summary>
    /// <param name="firstRender">Indicates whether this is the first render of the component.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <remarks>
    /// Design decision: Data loading occurs in OnAfterRenderAsync instead of OnInitializedAsync
    /// to avoid blocking the initial render and provide a better user experience with loading states.
    /// </remarks>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        try
        {
            // Retrieve the flood report ID from session storage
            _floodReportId = await scopedSessionStorage.GetFloodReportId();

            // Load the subscription record for the contact
            _subscribeModel = await contactRepository.GetSubscriptionRecordById(ContactId, _cts.Token);
            if (_subscribeModel is not null)
            {
                // Map subscription data to contact model for editing
                _contactModel!.EmailAddress = _subscribeModel.EmailAddress;
                _contactModel.IsEmailVerified = _subscribeModel.IsEmailVerified;
                _contactModel.ContactName = _subscribeModel.ContactName;
                _contactModel.ContactType = _subscribeModel.ContactType;
                _contactModel.Id = _subscribeModel.Id;
                _contactModel.IsRecordOwner = _subscribeModel.IsRecordOwner;
                _contactModel.PhoneNumber = _subscribeModel.PhoneNumber;
                _contactModel.ContactUserId = _subscribeModel.ContactRecordId;
            }

            // Load available contact types for the dropdown
            await LoadAvailableContactTypesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error loading contact data for ContactId {ContactId}", ContactId);
        }
        finally
        {
            // Update loading states and trigger re-render
            _isDataLoading = false;
            _isLoading = false;
            StateHasChanged();
        }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Loads the available contact types that can be selected for this contact.
    /// Excludes types already in use for the current flood report, except the current contact's type.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <remarks>
    /// Design decision: Only one contact of each type is allowed per flood report to ensure
    /// clear ownership and responsibility for flood-related communications.
    /// </remarks>
    private async Task LoadAvailableContactTypesAsync()
    {
        // Get contact types not currently used in this flood report
        var unusedTypes = await contactRepository.GetUnusedRecordTypes(_floodReportId, _cts.Token);
        var allTypes = Enum.GetValues<ContactRecordType>();
        var availableTypes = new List<ContactRecordType>();

        foreach (var type in allTypes)
        {
            // Include unused types and the current contact's type (to allow changing other fields)
            if (unusedTypes.Contains(type) || type == _contactModel!.ContactType)
            {
                availableTypes.Add(type);
            }
        }

        // Convert to GDS option items for rendering in the form
        ContactTypes = availableTypes.Select(CreateOption).ToArray();
    }

    /// <summary>
    /// Handles form submission.
    /// Validates the contact model and updates the contact if validation succeeds.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <remarks>
    /// Design decision: Manual validation is triggered on submit to avoid validation errors
    /// appearing prematurely for unauthenticated users filling out the form.
    /// </remarks>
    private async Task OnSubmit()
    {
        _messageStore.Clear();

        // Manual FluentValidation - only runs on submit to avoid premature validation errors
        var validator = new ContactModelValidator();
        var validationResult = await validator.ValidateAsync(_contactModel, _cts.Token);

        if (!validationResult.IsValid)
        {
            // Add FluentValidation errors to EditContext for display in the UI
            foreach (var error in validationResult.Errors)
            {
                var fieldIdentifier = _editContext.Field(error.PropertyName);
                _messageStore.Add(fieldIdentifier, error.ErrorMessage);
            }

            _editContext.NotifyValidationStateChanged();
            StateHasChanged();
            return;
        }

        await UpdateContactAsync();
    }

    /// <summary>
    /// Updates the contact information in the database.
    /// If the email is unverified, triggers a verification email to be sent.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <remarks>
    /// Design decision: Email verification is automatically triggered when an unverified email
    /// is updated to ensure contact information accuracy for flood-related communications.
    /// </remarks>
    private async Task UpdateContactAsync()
    {
        logger.LogDebug("Updating contact information for ContactId {ContactId}", ContactId);

        if (_contactModel is null)
        {
            logger.LogWarning("Contact model is null, cannot update contact");
            return;
        }

        try
        {
            // Retrieve the current subscription record
            var selectedRecord = await contactRepository.GetSubscriptionRecordById(ContactId, _cts.Token);
            if (selectedRecord is null)
            {
                logger.LogWarning("Subscription record not found for ContactId {ContactId}", ContactId);
                return;
            }

            // Update subscription record with new values from the form
            selectedRecord.PhoneNumber = _contactModel.PhoneNumber;
            selectedRecord.EmailAddress = _contactModel.EmailAddress!;
            selectedRecord.ContactName = _contactModel.ContactName!;
            selectedRecord.ContactType = _contactModel.ContactType!.Value;

            var updatedSubscription = await contactRepository.UpdateSubscriptionRecord(selectedRecord, _cts.Token);

            logger.LogInformation("Contact information updated successfully for ContactId {ContactId}, UserId {UserId}", ContactId, _userId);

            if (updatedSubscription.ResultModel is not SubscribeRecord sub)
            {
                logger.LogError("Failed to update subscription record for ContactId {ContactId}", ContactId);
                return;
            }

            // Handle email verification if the email is not yet verified
            if (!sub.IsEmailVerified)
            {
                await SendEmailVerificationAsync(sub);
            }

            // Navigate back to contacts summary page
            navigationManager.NavigateTo(ContactPages.Summary.Url);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating contact information for ContactId {ContactId}", ContactId);
            _messageStore.Add(_editContext.Field(nameof(_contactModel.ContactType)), "There was a problem updating the contact information. Please try again but if this issue persists then please report a bug.");
            _editContext.NotifyValidationStateChanged();
        }
    }

    /// <summary>
    /// Sends an email verification notification to the contact.
    /// Updates the verification code and expiry time before sending the email.
    /// </summary>
    /// <param name="subscription">The subscription record for which to send verification.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <remarks>
    /// TODO: The verification link generation needs to be implemented properly.
    /// Currently using placeholder values for requesterName and verificationLink.
    /// </remarks>
    private async Task SendEmailVerificationAsync(SubscribeRecord subscription)
    {
        try
        {
            // Update the verification code and expiry (userPresent: false means automatic send)
            var updatedVerification = await contactRepository.UpdateVerificationCode(subscription, userPresent: false, _cts.Token);

            if (updatedVerification.ResultModel is not SubscribeRecord returnedSubscription)
            {
                logger.LogWarning("Failed to update verification code for subscription {SubscriptionId}", subscription.Id);
                return;
            }

            if (returnedSubscription.VerificationExpiryUtc is not DateTimeOffset expiry)
            {
                logger.LogWarning("Verification expiry is null for subscription {SubscriptionId}", subscription.Id);
                return;
            }

            logger.LogInformation("Sending email verification notification to {EmailAddress}", returnedSubscription.EmailAddress);

            // TODO: Implement proper verification link generation
            // The verification link should include the verification code and subscription ID
            // Example: /verify/{subscriptionId}?code={verificationCode}
            await govNotifyEmailSender.SendEmailVerificationLinkNotification(
                returnedSubscription.EmailAddress,
                returnedSubscription.ContactName,
                requesterName: "Unknown",
                verificationLink: "To fix",
                expiry
            );

            _isResent = true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending email verification notification for subscription {SubscriptionId}", subscription.Id);
        }
    }

    /// <summary>
    /// Creates a GDS option item for a contact record type.
    /// </summary>
    /// <param name="contactRecordType">The contact record type to convert.</param>
    /// <returns>A <see cref="GdsOptionItem{T}"/> for use in a radio button or select list.</returns>
    /// <remarks>
    /// Uses extension methods LabelText() and HintText() to get localized display text
    /// for the contact record type.
    /// </remarks>
    private static GdsOptionItem<ContactRecordType> CreateOption(ContactRecordType contactRecordType)
    {
        var id = contactRecordType.ToString().AsSpan();
        var selected = false;
        return new GdsOptionItem<ContactRecordType>(id, contactRecordType.LabelText(), contactRecordType, selected, hint: contactRecordType.HintText());
    }

    #endregion
}

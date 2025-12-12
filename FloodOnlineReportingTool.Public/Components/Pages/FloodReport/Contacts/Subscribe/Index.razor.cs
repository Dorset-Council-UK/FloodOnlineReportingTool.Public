using FloodOnlineReportingTool.Contracts.Shared;
using FloodOnlineReportingTool.Database.Models.Contact;
using FloodOnlineReportingTool.Database.Models.Contact.Subscribe;
using FloodOnlineReportingTool.Database.Models.Flood;
using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.Public.Models.FloodReport.Contact;
using FloodOnlineReportingTool.Public.Models.FloodReport.Contact.Subscribe;
using FloodOnlineReportingTool.Public.Models.Order;
using FloodOnlineReportingTool.Public.Services;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport.Contacts.Subscribe;

public partial class Index(
    ILogger<Index> logger,
    IContactRecordRepository contactRepository,
    IGovNotifyEmailSender govNotifyEmailSender,
    NavigationManager navigationManager,
    SessionStateService scopedSessionStorage,
    ICurrentUserService currentUserService,
    IGdsJsInterop gdsJs
) : IPageOrder, IAsyncDisposable
{
    private readonly CancellationTokenSource _cts = new();
    private Guid _verificationId = Guid.Empty;
    private Guid _floodReportId = Guid.Empty;
    private Guid? _userID = null;
    private bool _isLoading = true;

    [SupplyParameterFromQuery]
    private bool Me { get; set; }

    [SupplyParameterFromQuery]
    private bool Owns { get; set; }

    [CascadingParameter]
    public Task<AuthenticationState>? AuthenticationState { get; set; }

    [SupplyParameterFromQuery]
    private bool FromSummary { get; set; }

    private EditContext _editContext = default!;

    public IReadOnlyCollection<GdsOptionItem<ContactRecordType>> ContactTypes = [];

    public required SubscribeModel Model { get; set; } = default!;

    private ValidationMessageStore _messageStore = default!;

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
            var user = authState.User;

            var oidClaim = user.FindFirst("oid")?.Value;
            _userID = Guid.TryParse(oidClaim, out var parsedOid) ? parsedOid : null;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _verificationId = await scopedSessionStorage.GetVerificationId();

            if (Me)
            {
                if (!currentUserService.IsAuthenticated || _userID is not Guid userId)
                {
                    // Can't proceed if not authenticated
                    navigationManager.NavigateTo(GeneralPages.Home.Url);
                    return;
                }
                // Check if this user already has a contact record
                var contactRecord = await contactRepository.ContactRecordExistsForUser(userId, _cts.Token);
                if (contactRecord is Guid contactRecordId)
                {
                    // Connect to the existing record and skip the subscription setup steps
                    _floodReportId = await scopedSessionStorage.GetFloodReportId();

                    var linkResult = await contactRepository.LinkContactByReport(_floodReportId, contactRecordId, _cts.Token);
                    if (linkResult.IsSuccess)
                    { 
                        // Proceed to summary
                        var nextPageUrl = ContactPages.Summary.Url;
                        navigationManager.NavigateTo(nextPageUrl);
                        return;
                    }
                }

                // Pre-fill email if known
                if (string.IsNullOrWhiteSpace(Model.ContactName))
                {
                    Model.ContactName = currentUserService.Name;
                }

                if (string.IsNullOrWhiteSpace(Model.EmailAddress))
                {
                    Model.EmailAddress = currentUserService.Email;
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

        // Error handling back into the Razor. Use validation message store or ErrorBoundary

        // Generate a contact record
        _floodReportId = await scopedSessionStorage.GetFloodReportId();
        ContactRecordDto dto = new ContactRecordDto
        {
            UserId = Model.ContactRecord!.ContactUserId,
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
            contactRecordId = newRecord.ContactRecord!.Id;
        } else
        {
            contactRecordId = contactRecord.First().Id;
        }
        SubscribeCreateOrUpdateResult subscriptionResult = await contactRepository.CreateSubscriptionRecord(contactRecordId, dto, currentUserService.Email, _cts.Token);
        
        if (subscriptionResult.ContactSubscriptionRecord is not SubscribeRecord returnedSubscription)
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
        } else
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

    private GdsOptionItem<ContactRecordType> CreateOption(ContactRecordType contactRecordType)
    {
        var id = contactRecordType.ToString().AsSpan();
        var selected = false;
        return new GdsOptionItem<ContactRecordType>(id, contactRecordType.LabelText(), contactRecordType, selected, hint: contactRecordType.HintText());
    }
}
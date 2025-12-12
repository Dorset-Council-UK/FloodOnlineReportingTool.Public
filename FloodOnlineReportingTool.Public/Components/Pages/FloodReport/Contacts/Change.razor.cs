using FloodOnlineReportingTool.Database.Models;
using FloodOnlineReportingTool.Database.Models.Contact.Subscribe;
using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.Public.Models.FloodReport.Contact;
using FloodOnlineReportingTool.Public.Models.Order;
using FloodOnlineReportingTool.Public.Services;
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
    IGovNotifyEmailSender govNotifyEmailSender,
    IGdsJsInterop gdsJs
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

    private ContactModel? _contactModel;
    private EditContext _editContext = default!;
    private SubscribeRecord? _subscribeModel;
    private Guid _floodReportId = Guid.Empty;
    private Guid _userID = Guid.Empty;
    private string _floodReportReference = string.Empty;
    private Database.Models.Flood.FloodReport? _floodReport;
    private bool _isLoading = true;
    private bool _isDataLoading = true;
    private bool isResent = false;
    private ValidationMessageStore _messageStore = default!;
    private readonly CancellationTokenSource _cts = new();
    private Guid _userId;

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
            _userID = Guid.TryParse(oidClaim, out var parsedOid) ? parsedOid : Guid.Empty;
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
                // Update the existing _contactModel instead of creating a new one
                _contactModel!.EmailAddress = _subscribeModel!.EmailAddress;
                _contactModel.IsEmailVerified = _subscribeModel.IsEmailVerified;
                _contactModel.ContactName = _subscribeModel.ContactName;
                _contactModel.ContactType = _subscribeModel.ContactType;
                _contactModel.Id = _subscribeModel.Id;
                _contactModel.IsRecordOwner = _subscribeModel.IsRecordOwner;
                _contactModel.PhoneNumber = _subscribeModel.PhoneNumber;
                _contactModel.ContactUserId = _subscribeModel.ContactRecordId;
            }

            _isDataLoading = false;
            _isLoading = false;
            InvokeAsync(StateHasChanged); // TODO blazor state error.
            await gdsJs.InitGds(_cts.Token);
        }
        
    }

    private async Task OnSubmit()
    {
        _messageStore.Clear();

        if (!_editContext.Validate())
        {
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
                    var sentNotification = await govNotifyEmailSender.SendEmailVerificationLinkNotification(
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

    // TODO - enable this once notification is available
    //if (!resultingContact.IsEmailVerified)
    //{
    //    // Resend verification email if it was changed
    //    var sentNotification2 = await govNotifyEmailSender.SendEmailVerificationNotification(
    //    _contactModel.ContactType!.Value.ToString(),
    //    _contactModel.PrimaryContactRecord,
    //     true,
    //    _contactModel.EmailAddress!,
    //    _contactModel.PhoneNumber!,
    //    _contactModel.ContactName!,
    //    _floodReport.Reference,
    //    _floodReport.EligibilityCheck!.LocationDesc ?? "",
    //    _floodReport.EligibilityCheck!.Easting,
    //    _floodReport.EligibilityCheck!.Northing,
    //    _floodReport.CreatedUtc
    //    );
    //}

    private async Task<ContactModel?> GetContact()
    {
        var floodReport = await floodReportRepository.ReportedByContact(_userId, ContactId, _cts.Token);
        if (floodReport == null)
        {
            return null;
        }

        //If we have a valid match then we return the reference for the current flood report only
        _floodReportReference = floodReport!.Reference;
        var contactRecord = floodReport.ContactRecords
            .Where(fr => fr.Id == ContactId)
            .FirstOrDefault();

        if (contactRecord == null)
        {
            return null;
        }
        return contactRecord.ToContactModel();
    }
}

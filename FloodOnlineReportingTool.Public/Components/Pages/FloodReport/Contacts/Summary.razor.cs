using FloodOnlineReportingTool.Contracts.Shared;
using FloodOnlineReportingTool.Database.Models;
using FloodOnlineReportingTool.Database.Models.Flood;
using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.Public.Models.FloodReport.Contact;
using FloodOnlineReportingTool.Public.Models.FloodReport.Contact.Subscribe;
using FloodOnlineReportingTool.Public.Models.Order;
using FloodOnlineReportingTool.Public.Services;
using GdsBlazorComponents;
using MassTransit.Initializers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using System.Runtime.CompilerServices;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport.Contacts;

public partial class Summary(
    ILogger<Summary> logger,
    IContactRecordRepository contactRepository,
    IFloodReportRepository floodReportRepository,
    NavigationManager navigationManager,
    SessionStateService scopedSessionStorage,
    IGovNotifyEmailSender govNotifyEmailSender
) : IPageOrder, IAsyncDisposable
{
    private readonly CancellationTokenSource _cts = new();
    private Guid _floodReportId = Guid.Empty;
    private Guid _verificationId = Guid.Empty;
    private bool _isLoading = true;
    private ContactModel? _reportOwnerContact;
    private IReadOnlyCollection<ContactModel> _contactModels = [];
    private int _numberOfUnusedRecordTypes;

    private EditContext _editContext = default!;

    private SubscribeModel Model { get; set; } = default!;

    private ValidationMessageStore _messageStore = default!;

    // Page order properties
    public string Title { get; set; } = ContactPages.Summary.Title;
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
            _floodReportId = await scopedSessionStorage.GetFloodReportId();

            await LoadContactData();

            _isLoading = false;
            StateHasChanged();
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        // This runs every time parameters change OR when navigating back to the page
        if (_floodReportId != Guid.Empty && !_isLoading)
        {
            await LoadContactData();
            StateHasChanged();
        }
    }

    private async Task LoadContactData()
    {
        var reportOwnerSubscribeRecord = await contactRepository.GetReportOwnerContactByReport(_floodReportId, _cts.Token);
        _reportOwnerContact = reportOwnerSubscribeRecord?.ToContactModel();

        var allContactRecords = await contactRepository.GetContactsByReport(_floodReportId, _cts.Token);

        // Filter out the report owner from the additional contacts list
        _contactModels = allContactRecords
            .SelectMany(cr => cr.SubscribeRecords)
            .Where(sr => !sr.IsRecordOwner)
            .Select(sr => sr.ToContactModel())
            .ToList();
        _numberOfUnusedRecordTypes = await contactRepository.CountUnusedRecordTypes(_floodReportId, _cts.Token);
    }

    private async Task OnSubmit()
    {

        var EnableSubscriptions = await floodReportRepository.EnableContactSubscriptionsForReport(_floodReportId, _cts.Token);

        if (!EnableSubscriptions.IsSuccess)
        {
            logger.LogError("Failed to enable contact subscriptions for flood report {FloodReportId}: {Errors}", _floodReportId, EnableSubscriptions.Errors);
            _messageStore.Add(new FieldIdentifier(Model, nameof(Model.ErrorMessage)), "An error occurred while updating your subscription preferences. Please try again.");
            _editContext.NotifyValidationStateChanged();
            return;
        }

        // Carry on whether this works or not
        try
        {
            // TODO: move the email logic to the service bus to allow for retries and better handling
            // Send message with the details requesting email to be sent but don't actually send it here
            foreach (var contactRecord in EnableSubscriptions.FloodReport!.ContactRecords)
            {
                foreach (var subscriptionRecord in contactRecord.SubscribeRecords)
                {
                    if (!subscriptionRecord.IsSubscribed || !subscriptionRecord.IsEmailVerified)
                    {
                        // Don't send email if not subscribed or email not verified
                        continue;
                    }

                    bool canEdit = false;
                    if (contactRecord.ContactUserId != null && contactRecord.ContactUserId != Guid.Empty)
                    {
                        canEdit = subscriptionRecord.IsRecordOwner;
                    }

                    if (subscriptionRecord.IsRecordOwner)
                    {
                        var sentNotification = await govNotifyEmailSender.SendReportSubmittedNotification(
                            subscriptionRecord.IsRecordOwner,
                            canEdit,
                            EnableSubscriptions.FloodReport.Reference,
                            subscriptionRecord.ContactType!.LabelText(),
                            subscriptionRecord.ContactName!,
                            subscriptionRecord.EmailAddress!,
                            EnableSubscriptions.FloodReport.EligibilityCheck!.LocationDesc ?? "",
                            EnableSubscriptions.FloodReport.EligibilityCheck!.Easting,
                            EnableSubscriptions.FloodReport.EligibilityCheck!.Northing,
                            EnableSubscriptions.FloodReport.CreatedUtc
                            );
                    } else
                    {
                        var sentNotification = await govNotifyEmailSender.SendReportSubmittedCopyNotification(
                            EnableSubscriptions.FloodReport.Reference,
                            subscriptionRecord.ContactType!.LabelText(),
                            subscriptionRecord.ContactName!,
                            subscriptionRecord.EmailAddress!,
                            EnableSubscriptions.FloodReport.EligibilityCheck!.LocationDesc ?? "",
                            EnableSubscriptions.FloodReport.EligibilityCheck!.Easting,
                            EnableSubscriptions.FloodReport.EligibilityCheck!.Northing,
                            EnableSubscriptions.FloodReport.CreatedUtc
                            );
                    }
                        
                }
            }
        } catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send subscription notification emails for flood report {FloodReportId}", _floodReportId);
        }

        // Navigate back to flood report overview
        navigationManager.NavigateTo(FloodReportPages.Overview.Url);
    }
}
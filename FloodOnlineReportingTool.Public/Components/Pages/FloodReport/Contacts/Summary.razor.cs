using FloodOnlineReportingTool.Contracts.Shared;
using FloodOnlineReportingTool.Database.Models;
using FloodOnlineReportingTool.Database.Models.Flood;
using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.Public.Models.FloodReport.Contact;
using FloodOnlineReportingTool.Public.Models.FloodReport.Contact.Subscribe;
using FloodOnlineReportingTool.Public.Models.Order;
using FloodOnlineReportingTool.Public.Services;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport.Contacts;

public partial class Summary(
    ILogger<Summary> logger,
    IContactRecordRepository contactRepository,
    IFloodReportSourceRepository floodReportSourceRepository,
    ISubscribeRecordRepository subscribeRecordRepository,
    NavigationManager navigationManager,
    SessionStateService scopedSessionStorage,
    IGovNotifyEmailSender govNotifyEmailSender
) : IPageOrder, IAsyncDisposable
{
    private readonly CancellationTokenSource _cts = new();
    private Guid _floodReportSourceId = Guid.Empty;
    private bool _isLoading = true;
    private ContactModel? _reportOwnerContact;
    private IReadOnlyCollection<ContactModel> _contactModels = [];
    private int _numberOfUnusedRecordTypes;

    private EditContext _editContext = default!;

    [Parameter]
    public Guid? FloodReportSourceId { get; set; }

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
            _floodReportSourceId = FloodReportSourceId ?? await scopedSessionStorage.GetFloodReportSourceId();

            await LoadContactData();

            _isLoading = false;
            StateHasChanged();
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        // This runs every time parameters change OR when navigating back to the page
        if (_floodReportSourceId != Guid.Empty && !_isLoading)
        {
            await LoadContactData();
            StateHasChanged();
        }
    }

    private async Task LoadContactData()
    {
        var reportOwnerSubscribeRecord = await subscribeRecordRepository.GetReportOwnerContactByReport(_floodReportSourceId, _cts.Token);
        _reportOwnerContact = reportOwnerSubscribeRecord?.ToContactModel();

        if (_reportOwnerContact is null)
        {
            // This is not allowed, setup an owner
            navigationManager.NavigateTo($"{SubscriptionPages.Home.Url}?Owns=true");
            return;
        }

        var allContactRecords = await contactRepository.GetContactsByReport(_floodReportSourceId, _cts.Token);

        // Filter out the report owner from the additional contacts list
        _contactModels = [.. allContactRecords
            .SelectMany(cr => cr.SubscribeRecords)
            .Where(sr => !sr.IsRecordOwner)
            .Select(sr => sr.ToContactModel()),
         ];
        _numberOfUnusedRecordTypes = await contactRepository.CountUnusedRecordTypes(_floodReportSourceId, _cts.Token);
    }

    private async Task OnSubmit()
    {

        var enableSubscriptionsResult = await floodReportSourceRepository.EnableContactSubscriptionsForReport(_floodReportSourceId, _cts.Token);

        if (!enableSubscriptionsResult.IsSuccess)
        {
            logger.LogError("Failed to enable contact subscriptions for flood report source {FloodReportSourceId}", _floodReportSourceId);
            foreach (var error in enableSubscriptionsResult.Errors)
            {
                logger.LogError("Enable contact subscriptions error: {Error}", error);
            }
            _messageStore.Add(new FieldIdentifier(Model, nameof(Model.ErrorMessage)), "An error occurred while updating your subscription preferences. Please try again.");
            _editContext.NotifyValidationStateChanged();
            return;
        }

        // Carry on whether this works or not
        try
        {
            FloodReportSource floodReportSource = enableSubscriptionsResult.Value;
            // TODO: move the email logic to the service bus to allow for retries and better handling
            // Send message with the details requesting email to be sent but don't actually send it here
            foreach (var contactRecord in floodReportSource.ContactRecords)
            {
                foreach (var subscriptionRecord in contactRecord.SubscribeRecords)
                {
                    if (!subscriptionRecord.IsSubscribed || !subscriptionRecord.IsEmailVerified)
                    {
                        // Don't send email if not subscribed or email not verified
                        continue;
                    }

                    bool canEdit = !string.IsNullOrWhiteSpace(contactRecord.ContactUserId) && subscriptionRecord.IsRecordOwner;

                    if (subscriptionRecord.IsRecordOwner)
                    {
                        var sentNotification = await govNotifyEmailSender.SendReportSubmittedNotification(
                            subscriptionRecord.IsRecordOwner,
                            canEdit,
                            floodReportSource.Reference,
                            subscriptionRecord.ContactType.LabelText(),
                            subscriptionRecord.ContactName,
                            subscriptionRecord.EmailAddress,
                            floodReportSource.EligibilityCheck?.LocationDesc ?? "",
                            floodReportSource.EligibilityCheck!.Easting,
                            floodReportSource.EligibilityCheck!.Northing,
                            floodReportSource.CreatedUtc
                            );
                    } else
                    {
                        var sentNotification = await govNotifyEmailSender.SendReportSubmittedCopyNotification(
                            floodReportSource.Reference,
                            subscriptionRecord.ContactType.LabelText(),
                            subscriptionRecord.ContactName,
                            subscriptionRecord.EmailAddress,
                            floodReportSource.EligibilityCheck?.LocationDesc ?? "",
                            floodReportSource.EligibilityCheck!.Easting,
                            floodReportSource.EligibilityCheck!.Northing,
                            floodReportSource.CreatedUtc
                            );
                    }
                        
                }
            }
        } catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send subscription notification emails for flood report source {FloodReportSourceId}", _floodReportSourceId);
        }

        // Navigate back to flood report overview page
        navigationManager.NavigateTo(FloodReportPages.Overview.Url);
    }
}
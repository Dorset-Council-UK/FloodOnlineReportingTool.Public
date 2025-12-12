using FloodOnlineReportingTool.Database.Models;
using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.Public.Models.FloodReport.Contact;
using FloodOnlineReportingTool.Public.Models.FloodReport.Contact.Subscribe;
using FloodOnlineReportingTool.Public.Models.Order;
using FloodOnlineReportingTool.Public.Services;
using GdsBlazorComponents;
using MassTransit.Initializers;
using Microsoft.AspNetCore.Components.Forms;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport.Contacts;

public partial class Summary(
    ILogger<Summary> logger,
    IContactRecordRepository contactRepository,
    IFloodReportRepository floodReportRepository,
    SessionStateService scopedSessionStorage,
    IGdsJsInterop gdsJs
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
            await gdsJs.InitGds(_cts.Token);
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
        if (!_editContext.Validate())
        {
            return;
        }

        return;
    }
}
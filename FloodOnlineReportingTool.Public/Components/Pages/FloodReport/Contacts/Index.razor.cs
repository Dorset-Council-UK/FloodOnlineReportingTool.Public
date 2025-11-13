using FloodOnlineReportingTool.Database.Models;
using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.Public.Models.FloodReport.Contact;
using FloodOnlineReportingTool.Public.Models.Order;
using FloodOnlineReportingTool.Public.Services;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport.Contacts;

public partial class Index(
    IContactRecordRepository contactRepository,
    IFloodReportRepository floodReportRepository,
    SessionStateService scopedSessionStorage,
    IGdsJsInterop gdsJs
) : IPageOrder, IAsyncDisposable
{
    // Page order properties
    public string Title { get; set; } = ContactPages.Home.Title;
    public IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [
        GeneralPages.Home.ToGdsBreadcrumb(),
        FloodReportPages.Overview.ToGdsBreadcrumb(),
    ];

    [CascadingParameter]
    public Task<AuthenticationState>? AuthenticationState { get; set; }

    private readonly CancellationTokenSource _cts = new();
    private Guid _floodReportId = Guid.Empty;
    private bool _isLoading = true;
    private IReadOnlyCollection<ContactModel> _contactModels = [];
    private int _numberOfUnusedRecordTypes;

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

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _floodReportId = await scopedSessionStorage.GetFloodReportId();

            var userId = await AuthenticationState.IdentityUserId();
            if (userId != null)
            {
                var _floodReports = await floodReportRepository.AllReportedByContact(userId.Value, _cts.Token);
                _contactModels = [.. _floodReports.SelectMany(fc => fc.ContactRecords).Select(o => o.ToContactModel())];
                _numberOfUnusedRecordTypes = await contactRepository.CountUnusedRecordTypes(_floodReportId, _cts.Token);
            }
            else
            {
                var _floodReports = await contactRepository.GetContactsByReport(_floodReportId, _cts.Token);
                _contactModels = [.. _floodReports.Select(o => o.ToContactModel())];
                _numberOfUnusedRecordTypes = await contactRepository.CountUnusedRecordTypes(_floodReportId, _cts.Token);
            }

            _isLoading = false;
            StateHasChanged();
            await gdsJs.InitGds(_cts.Token);
        }


    }
}

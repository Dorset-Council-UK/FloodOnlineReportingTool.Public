using FloodOnlineReportingTool.Database.Models;
using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.Public.Models.FloodReport.Contact;
using FloodOnlineReportingTool.Public.Models.Order;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport.Contacts;

[Authorize]
public partial class Index(
    IContactRecordRepository contactRepository,
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

    protected override async Task OnInitializedAsync()
    {
        var userId = await AuthenticationState.IdentityUserId();
        if (userId != null)
        {
            var _floodReports = await contactRepository.AllReportedByUser(userId.Value, _cts.Token).ConfigureAwait(false);
            _contactModels = [.. _floodReports.SelectMany(fc => fc.ExtraContactRecords).Select(o => o.ToContactModel())];
            _numberOfUnusedRecordTypes = await contactRepository.CountUnusedRecordTypes(userId.Value, _cts.Token).ConfigureAwait(false);
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await gdsJs.InitGds(_cts.Token);
        }
    }
}

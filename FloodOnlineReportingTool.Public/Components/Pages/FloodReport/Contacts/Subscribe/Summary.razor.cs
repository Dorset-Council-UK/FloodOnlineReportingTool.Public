using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.Public.Models.Order;
using FloodOnlineReportingTool.Public.Services;
using GdsBlazorComponents;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport.Contacts.Subscribe;

public partial class Summary(
    IContactRecordRepository contactRepository,
    IFloodReportRepository floodReportRepository,
    SessionStateService scopedSessionStorage,
    IGdsJsInterop gdsJs
) : IPageOrder, IAsyncDisposable
{
    private readonly CancellationTokenSource _cts = new();
    private Guid _floodReportId = Guid.Empty;
    private bool _isLoading = true;

    // Page order properties
    public string Title { get; set; } = SubscriptionPages.Summary.Title;
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

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _floodReportId = await scopedSessionStorage.GetFloodReportId();

            _isLoading = false;
            StateHasChanged();
            await gdsJs.InitGds(_cts.Token);
        }
    }
}
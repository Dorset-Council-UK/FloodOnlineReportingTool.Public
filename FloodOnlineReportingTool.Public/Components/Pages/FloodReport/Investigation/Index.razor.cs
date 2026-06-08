using FloodOnlineReportingTool.Database.Models.Investigate;
using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.Public.Models;
using FloodOnlineReportingTool.Public.Models.Order;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport.Investigation;

[Authorize]
public partial class Index(
    ProtectedSessionStorage protectedSessionStorage,
    IFloodReportSourceRepository floodReportSourceRepository
) : IPageOrder, IAsyncDisposable
{
    // Page order properties
    public string Title { get; set; } = InvestigationPages.Home.Title;
    public IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [
        GeneralPages.Home.ToGdsBreadcrumb(),
        FloodReportPages.Overview.ToGdsBreadcrumb(),
    ];

    [Parameter]
    public Guid FloodReportSourceId { get; set; }

    [CascadingParameter]
    public Task<AuthenticationState>? AuthenticationState { get; set; }

    private readonly CancellationTokenSource _cts = new();
    private bool _hasFloodReportSource;
    private bool _hasInvestigation;
    private bool _hasInvestigationStarted;
    private DateTimeOffset? _investigationCreatedUtc;

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
        (_hasFloodReportSource, _hasInvestigation, _hasInvestigationStarted, _investigationCreatedUtc) = await floodReportSourceRepository.InvestigationBasicInformation(FloodReportSourceId, _cts.Token);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            if (!_hasInvestigationStarted)
            {
                //We start it here
                InvestigationDto investigation = new InvestigationDto() with
                {
                    FloodReportSourceId = FloodReportSourceId,
                };
                await protectedSessionStorage.SetAsync(SessionConstants.Investigation, investigation);
            }
        }
    }
}

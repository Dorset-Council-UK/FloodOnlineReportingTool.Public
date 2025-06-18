﻿using FloodOnlineReportingTool.DataAccess.Repositories;
using FloodOnlineReportingTool.GdsComponents;
using FloodOnlineReportingTool.Public.Models.Order;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport.Investigation;

[Authorize]
public partial class Index(
    IFloodReportRepository floodReportRepository,
    IJSRuntime JS
) : IPageOrder, IAsyncDisposable
{
    // Page order properties
    public string Title { get; set; } = InvestigationPages.Home.Title;
    public IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [
        GeneralPages.Home.ToGdsBreadcrumb(),
        FloodReportPages.Overview.ToGdsBreadcrumb(),
    ];

    [CascadingParameter]
    public Task<AuthenticationState>? AuthenticationState { get; set; }

    private readonly CancellationTokenSource _cts = new();
    private bool _hasFloodReport;
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
        var userId = await AuthenticationState.IdentityUserId();
        if (userId.HasValue)
        {
            (_hasFloodReport, _hasInvestigation, _hasInvestigationStarted, _investigationCreatedUtc) = await floodReportRepository.ReportedByUserBasicInformation(userId.Value, _cts.Token).ConfigureAwait(false);
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JS.InvokeVoidAsync("window.initGDS", _cts.Token);
        }
    }
}

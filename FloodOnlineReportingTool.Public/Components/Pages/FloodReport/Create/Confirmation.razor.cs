using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.Public.Models.Order;
using FloodOnlineReportingTool.Public.Services;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport.Create;

public partial class Confirmation(
    ILogger<Confirmation> logger,
    IEligibilityCheckRepository eligibilityRepository,
    IMediaItemRepository mediaItemRepository,
    IFloodReportSourceRepository floodReportSourceRepository,
    SessionStateService scopedSessionStorage
) : IPageOrder, IAsyncDisposable
{
    // Parameters
    [SupplyParameterFromQuery]
    private string? Reference { get; set; }

    // Private Fields
    private readonly CancellationTokenSource _cts = new();
    private bool _isLoading = true;
    private bool _loadingError;
    private Guid _FloodReportId;
    private bool _hasContactInformation;
    private int _mediaItemsAssociatedWithReport;
    
    // Public Properties
    public string Title { get; set; } = FloodReportCreatePages.Confirmation.Title;
    public IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [
        GeneralPages.Home.ToGdsBreadcrumb(),
        FloodReportPages.Home.ToGdsBreadcrumb(),
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
            // Suppressing exception during disposal to prevent issues during component teardown
        }

        GC.SuppressFinalize(this);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(Reference))
        {
            await LoadFromReferenceAsync();
        }
        else
        {
            await LoadFromSessionAsync();
        }

        _isLoading = false;
        StateHasChanged();
    }

    private async Task LoadFromReferenceAsync()
    {
        try
        {
            var result = await eligibilityRepository.GetByReference(Reference!, _cts.Token);
            var floodReportSource = result?.FloodReportSource;

            if (floodReportSource is null)
            {
                return;
            }

            _FloodReportId = floodReportSource.Id;

            if (_FloodReportId != Guid.Empty)
            {
                await scopedSessionStorage.SaveFloodReporSourceId(_FloodReportId);
                await LoadMediaItemCountAsync();
            }

            _hasContactInformation = floodReportSource.ContactRecords.Count > 0;
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "There was a problem getting the eligibility check from the database");
            _loadingError = true;
        }
    }

    private async Task LoadFromSessionAsync()
    {
        _FloodReportId = await scopedSessionStorage.GetFloodReportSourceId();

        if (_FloodReportId == Guid.Empty)
        {
            return;
        }

        var result = await floodReportSourceRepository.GetById(_FloodReportId, _cts.Token);
        Reference = result?.Reference;
        await LoadMediaItemCountAsync();
    }

    private async Task LoadMediaItemCountAsync()
    {
        _mediaItemsAssociatedWithReport = await mediaItemRepository.GetCountByReport(_FloodReportId, _cts.Token);
    }
}

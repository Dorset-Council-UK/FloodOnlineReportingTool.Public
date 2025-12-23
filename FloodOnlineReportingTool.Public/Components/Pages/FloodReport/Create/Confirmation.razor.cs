using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.Public.Models.Order;
using FloodOnlineReportingTool.Public.Services;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport.Create;

public partial class Confirmation(
    ILogger<Confirmation> logger,
    IEligibilityCheckRepository eligibilityRepository,
    SessionStateService scopedSessionStorage
) : IPageOrder, IAsyncDisposable
{
    // Page order properties
    public string Title { get; set; } = FloodReportCreatePages.Confirmation.Title;
    public IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [
        GeneralPages.Home.ToGdsBreadcrumb(),
        FloodReportPages.Home.ToGdsBreadcrumb(),
    ];

    [SupplyParameterFromQuery]
    private string? Reference { get; set; }

    private readonly CancellationTokenSource _cts = new();
    private bool _isLoading = true;
    private bool _loadingError;
    private Guid _FloodReportId;
    private bool _hasContactInformation;

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
            if (!string.IsNullOrWhiteSpace(Reference))
            {
                try
                {
                    var result = await eligibilityRepository.GetByReference(Reference, _cts.Token);

                    if ( result?.FloodReport != null)
                    {
                        _FloodReportId = result.FloodReport.Id;
                        // Store the current flood report to session storage
                        if (_FloodReportId != Guid.Empty)
                        {
                            //Never save a blank Guid, only a real one
                            await scopedSessionStorage.SaveFloodReportId(_FloodReportId);
                        }

                        _hasContactInformation = result.FloodReport.ContactRecords.Count > 0;
                    }
                }
                catch (InvalidOperationException ex)
                {
                    logger.LogError(ex, "There was a problem getting the eligibility check from the database");
                    _loadingError = true;
                }
            }

            _isLoading = false;
            StateHasChanged();

            
        }
    }
}

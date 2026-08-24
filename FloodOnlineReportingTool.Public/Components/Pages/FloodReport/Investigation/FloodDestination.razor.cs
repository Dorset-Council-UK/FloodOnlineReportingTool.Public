using FloodOnlineReportingTool.Database.Models.Flood;
using FloodOnlineReportingTool.Database.Models.Flood.FloodProblemIds;
using FloodOnlineReportingTool.Database.Models.Investigate;
using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.Public.Models;
using FloodOnlineReportingTool.Public.Models.FloodReport.Create;
using FloodOnlineReportingTool.Public.Models.Order;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport.Investigation;

[Authorize]
public partial class FloodDestination(
    ILogger<FloodDestination> logger,
    ICommonRepository commonRepository,
    ProtectedSessionStorage protectedSessionStorage,
    NavigationManager navigationManager
) : IAsyncDisposable
{
    // Page order properties
    public string Title { get; set; } = InvestigationPages.Speed.Title;

    [SupplyParameterFromQuery]
    private bool FromSummary { get; set; }
    private PageInfo NextPage => FromSummary
        ? InvestigationPages.Summary
        : InvestigationPages.Vehicles;
    private static PageInfo PreviousPage => InvestigationPages.Speed;

    private Models.FloodReport.Investigation.FloodDestination Model { get; set; } = default!;

    private EditContext _editContext = default!;
    private readonly CancellationTokenSource _cts = new();
    private bool _isLoading = true;
    private IList<FloodProblem> DestinationOptions { get; set; } = [];
    private Dictionary<string, bool> SelectedDestinationOptions = [];

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
        if (Model is null)
        {
            // Setup model and edit context
            Model ??= new();
            _editContext = new(Model);
            _editContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
        }

        DestinationOptions = await commonRepository.GetFloodProblemsByCategory(FloodProblemCategory.Destination, _cts.Token);
        UpdateSelectedDestinationOptions();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Set any previously entered data
            var investigation = await GetInvestigation();
            Model.DestinationOptions = [.. investigation.Destinations];
            UpdateSelectedDestinationOptions();

            _isLoading = false;
            StateHasChanged();  
        }
    }
    
    private async Task OnValidSubmit()
    {
        var investigation = await GetInvestigation();
        var updatedInvestigation = investigation with
        {
            Destinations = Model.DestinationOptions,
        };
        await protectedSessionStorage.SetAsync(SessionConstants.Investigation, updatedInvestigation);

        // Go to the next page or back to the summary
        navigationManager.NavigateTo(NextPage.Url);
    }

    private async Task<InvestigationDto> GetInvestigation()
    {
        var data = await protectedSessionStorage.GetAsync<InvestigationDto>(SessionConstants.Investigation);
        if (data.Success)
        {
            if (data.Value != null)
            {
                return data.Value;
            }
        }

        logger.LogWarning("Investigation was not found in the protected storage.");
        return new InvestigationDto();
    }

    /// <summary>
    /// Set up the selected destination options (string, bool dictionary)
    /// </summary>
    private void UpdateSelectedDestinationOptions()
    {
        SelectedDestinationOptions = DestinationOptions.ToDictionary(o => o.Id.ToString("N"), o => Model.DestinationOptions.Contains(o.Id), StringComparer.Ordinal);
    }

    private void OnDestinationChanged(bool isChecked, Guid floodSourceId)
    {
        // update the model
        if (isChecked && !Model.DestinationOptions.Contains(floodSourceId))
            Model.DestinationOptions.Add(floodSourceId);
        else if (!isChecked)
            Model.DestinationOptions.Remove(floodSourceId);
    }

}

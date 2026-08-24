using FloodOnlineReportingTool.Database.Models.Flood;
using FloodOnlineReportingTool.Database.Models.Investigate;
using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.Public.Models;
using FloodOnlineReportingTool.Public.Models.Order;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport.Investigation;

[Authorize]
public partial class CommunityImpact(
    ILogger<CommunityImpact> logger,
    ICommonRepository commonRepository,
    ProtectedSessionStorage protectedSessionStorage,
    NavigationManager navigationManager
) : IAsyncDisposable
{
    // Page order properties
    public string Title { get; set; } = InvestigationPages.CommunityImpact.Title;

    [SupplyParameterFromQuery]
    private bool FromSummary { get; set; }
    private PageInfo NextPage => FromSummary
        ? InvestigationPages.Summary
        : InvestigationPages.Blockages;
    private static PageInfo PreviousPage => InvestigationPages.ServiceImpact;

    private Models.FloodReport.Investigation.CommunityImpact Model { get; set; } = default!;

    private EditContext _editContext = default!;
    private readonly CancellationTokenSource _cts = new();
    private bool _isLoading = true;
    private IList<FloodImpact> CommunityImpactOptions = [];
    private Dictionary<string, bool> SelectedCommunityImpactOptions = [];

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

        CommunityImpactOptions = await commonRepository.GetFloodImpactsByCategory(FloodImpactCategory.CommunityImpact, _cts.Token);
        UpdateSelectedCommunityImpactOptions();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Set any previously entered data
            var investigation = await GetInvestigation();
            Model.CommunityImpactOptions = investigation.CommunityImpacts;
            UpdateSelectedCommunityImpactOptions();

            _isLoading = false;
            StateHasChanged(); 
        }
    }

    private async Task OnValidSubmit()
    {
        var investigation = await GetInvestigation();
        var updatedInvestigation = investigation with
        {
            CommunityImpacts = Model.CommunityImpactOptions,
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
    /// Set up the selected community impact options (string, bool dictionary)
    /// </summary>
    private void UpdateSelectedCommunityImpactOptions()
    {
        SelectedCommunityImpactOptions = CommunityImpactOptions.ToDictionary(o => o.Id.ToString("N"), o => Model.CommunityImpactOptions.Contains(o.Id), StringComparer.Ordinal);
    }

    private void OnCommunityImpactChanged(bool isChecked, Guid floodImpactId)
    {
        // update the model
        if (isChecked && !Model.CommunityImpactOptions.Contains(floodImpactId))
            Model.CommunityImpactOptions.Add(floodImpactId);
        else if (!isChecked)
            Model.CommunityImpactOptions.Remove(floodImpactId);
    }

}
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
public partial class WarningSources(
    ILogger<WarningSources> logger,
    ICommonRepository commonRepository,
    ProtectedSessionStorage protectedSessionStorage,
    NavigationManager navigationManager
) : IAsyncDisposable
{
    // Page order properties
    public string Title { get; set; } = InvestigationPages.WarningSources.Title;

    [SupplyParameterFromQuery]
    private bool FromSummary { get; set; }
    private static PageInfo PreviousPage => InvestigationPages.Warnings;

    private Models.FloodReport.Investigation.WarningSources Model { get; set; } = default!;

    private EditContext _editContext = default!;
    private readonly CancellationTokenSource _cts = new();
    private bool _isLoading = true;
    private IList<FloodMitigation> WarningSourceOptions = [];
    private Dictionary<string, bool> SelectedWarningSourceOptions = [];

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

        WarningSourceOptions = await commonRepository.GetFloodMitigationsByCategory(FloodMitigationCategory.WarningSource, _cts.Token);
        UpdateSelectedWarningSourceOptions();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Set any previously entered data
            var investigation = await GetInvestigation();
            Model.WarningSourceOptions = [.. investigation.WarningSources];
            Model.WarningOther = investigation.WarningSourceOther;
            UpdateSelectedWarningSourceOptions();

            _isLoading = false;
            StateHasChanged(); 
        }
    }

    private async Task OnValidSubmit()
    {
        var investigation = await GetInvestigation();
        var updatedInvestigation = investigation with
        {
            WarningSources = Model.WarningSourceOptions,
            WarningSourceOther = Model.WarningOther,
        };
        await protectedSessionStorage.SetAsync(SessionConstants.Investigation, updatedInvestigation);

        // Go to the next page or back to the summary
        navigationManager.NavigateTo(GetNextPage().Url);
    }

    private PageInfo GetNextPage()
    {
        if (FromSummary)
        {
            return InvestigationPages.Summary;
        }

        var isFloodLineWarningSelected = Model.WarningSourceOptions.Any(o => o.Equals(FloodMitigationIds.FloodlineWarning));
        return isFloodLineWarningSelected ? InvestigationPages.Floodline : InvestigationPages.History;
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
    /// Set up the selected warning sources options (string, bool dictionary)
    /// </summary>
    private void UpdateSelectedWarningSourceOptions()
    {
        SelectedWarningSourceOptions = WarningSourceOptions.ToDictionary(o => o.Id.ToString("N"), o => Model.WarningSourceOptions.Contains(o.Id), StringComparer.Ordinal);
    }

    private void OnWarningSourceChanged(bool isChecked, Guid floodMitigationId)
    {
        // update the model
        if (isChecked && !Model.WarningSourceOptions.Contains(floodMitigationId))
            Model.WarningSourceOptions.Add(floodMitigationId);
        else if (!isChecked)
            Model.WarningSourceOptions.Remove(floodMitigationId);
    }

}

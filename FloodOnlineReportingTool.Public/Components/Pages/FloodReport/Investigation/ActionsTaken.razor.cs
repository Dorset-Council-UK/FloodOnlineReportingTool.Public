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
public partial class ActionsTaken(
    ILogger<ActionsTaken> logger,
    ICommonRepository commonRepository,
    ProtectedSessionStorage protectedSessionStorage,
    NavigationManager navigationManager
) : IAsyncDisposable
{
    // Page order properties
    public string Title { get; set; } = InvestigationPages.ActionsTaken.Title;

    [SupplyParameterFromQuery]
    private bool FromSummary { get; set; }
    private PageInfo NextPage => FromSummary
        ? InvestigationPages.Summary
        : InvestigationPages.HelpReceived;
    private static PageInfo PreviousPage => InvestigationPages.Blockages;

    private Models.FloodReport.Investigation.ActionsTaken Model { get; set; } = default!;

    private EditContext _editContext = default!;
    private readonly CancellationTokenSource _cts = new();
    private bool _isLoading = true;
    private IList<FloodMitigation> ActionsTakenOptions = [];
    private Dictionary<string, bool> SelectedActionsTakenOptions = [];

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

        ActionsTakenOptions = await commonRepository.GetFloodMitigationsByCategory(FloodMitigationCategory.ActionsTaken, _cts.Token);
        UpdateSelectedActionsTakenOptions();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Set any previously entered data
            var investigation = await GetInvestigation();
            Model.ActionsTakenOptions = [.. investigation.ActionsTaken];
            Model.OtherAction = investigation.OtherAction;
            UpdateSelectedActionsTakenOptions();

            _isLoading = false;
            StateHasChanged(); 
        }
    }

    private async Task OnValidSubmit()
    {
        var investigation = await GetInvestigation();
        var updatedInvestigation = investigation with
        {
            ActionsTaken = Model.ActionsTakenOptions,
            OtherAction = Model.ActionsTakenOptions.Contains(FloodMitigationIds.OtherAction) ? Model.OtherAction : null,
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
    /// Set up the selected actions taken options (string, bool dictionary)
    /// </summary>
    private void UpdateSelectedActionsTakenOptions()
    {
        SelectedActionsTakenOptions = ActionsTakenOptions.ToDictionary(o => o.Id.ToString("N"), o => Model.ActionsTakenOptions.Contains(o.Id), StringComparer.Ordinal);
    }

    private void OnActionsTakenChanged(bool isChecked, Guid floodMitigationId)
    {
        // update the model
        if (isChecked && !Model.ActionsTakenOptions.Contains(floodMitigationId))
            Model.ActionsTakenOptions.Add(floodMitigationId);
        else if (!isChecked)
            Model.ActionsTakenOptions.Remove(floodMitigationId);
    }

}

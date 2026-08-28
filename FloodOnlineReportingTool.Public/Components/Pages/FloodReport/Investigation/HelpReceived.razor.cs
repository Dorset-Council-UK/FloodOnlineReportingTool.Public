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
public partial class HelpReceived(
    ILogger<HelpReceived> logger,
    ICommonRepository commonRepository,
    ProtectedSessionStorage protectedSessionStorage,
    NavigationManager navigationManager
) : IAsyncDisposable
{
    // Page order properties
    public string Title { get; set; } = InvestigationPages.HelpReceived.Title;

    [SupplyParameterFromQuery]
    private bool FromSummary { get; set; }
    private PageInfo NextPage => FromSummary
        ? InvestigationPages.Summary
        : InvestigationPages.Warnings;
    private static PageInfo PreviousPage => InvestigationPages.ActionsTaken;

    private Models.FloodReport.Investigation.HelpReceived Model { get; set; } = default!;

    private EditContext _editContext = default!;
    private readonly CancellationTokenSource _cts = new();
    private bool _isLoading = true;
    private IList<FloodMitigation> HelpReceivedOptions = [];
    private Dictionary<string, bool> SelectedHelpReceivedOptions = [];

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
        HelpReceivedOptions = await commonRepository.GetFloodMitigationsByCategory(FloodMitigationCategory.HelpReceived, _cts.Token);
        UpdateSelectedHelpReceivedOptions();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Set any previously entered data
            var investigation = await GetInvestigation();
            Model.HelpReceivedOptions = [.. investigation.HelpReceived];
            UpdateSelectedHelpReceivedOptions();

            _isLoading = false;
            StateHasChanged(); 
        }
    }

    private async Task OnValidSubmit()
    {
        var investigation = await GetInvestigation();
        var updatedInvestigation = investigation with
        {
            HelpReceived = Model.HelpReceivedOptions,
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
    /// Set up the selected help received options (string, bool dictionary)
    /// </summary>
    private void UpdateSelectedHelpReceivedOptions()
    {
        SelectedHelpReceivedOptions = HelpReceivedOptions.ToDictionary(o => o.Id.ToString("N"), o => Model.HelpReceivedOptions.Contains(o.Id), StringComparer.Ordinal);
    }

    private void OnHelpReceivedChanged(bool isChecked, Guid floodMitigationId)
    {
        // update the model
        if (isChecked && !Model.HelpReceivedOptions.Contains(floodMitigationId))
            Model.HelpReceivedOptions.Add(floodMitigationId);
        else if (!isChecked)
            Model.HelpReceivedOptions.Remove(floodMitigationId);
    }

}

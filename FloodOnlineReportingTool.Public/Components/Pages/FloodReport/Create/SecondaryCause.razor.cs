using FloodOnlineReportingTool.Contracts.Shared;
using FloodOnlineReportingTool.Database.Models.Eligibility;
using FloodOnlineReportingTool.Database.Models.Flood;
using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.Public.Models;
using FloodOnlineReportingTool.Public.Models.Order;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport.Create;

public partial class SecondaryCause(
    ILogger<SecondaryCause> logger,
    ICommonRepository commonRepository,
    ProtectedSessionStorage protectedSessionStorage,
    NavigationManager navigationManager
) : IAsyncDisposable
{
    private Models.FloodReport.Create.FloodSecondaryCause Model { get; set; } = default!;
    
    [SupplyParameterFromQuery]
    private bool FromSummary { get; set; }
    private static PageInfo NextPage => FloodReportCreatePages.Summary;
    private PageInfo PreviousPage => FromSummary
        ? FloodReportCreatePages.Summary
        : FloodReportCreatePages.Cause;

    private EditContext _editContext = default!;
    private readonly CancellationTokenSource _cts = new();
    private bool _isLoading = true;
    private IList<FloodProblem> SecondaryCauseOptions = [];
    private Dictionary<string, bool> SelectedSecondaryCauseOptions = [];

    protected override async Task OnInitializedAsync()
    {
        if (Model is null)
        {
            // Setup model and edit context
            Model ??= new();
            _editContext = new(Model);
            _editContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
        }

        SecondaryCauseOptions = await commonRepository.GetFloodProblemsByCategory(FloodProblemCategory.SecondaryCause, _cts.Token);
        UpdateSelectedSecondaryCauseOptions();
    }

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
            var eligibilityCheck = await GetEligibilityCheck();

            Model.SecondaryCauseOptions = [..eligibilityCheck.SecondaryCauses];
            UpdateSelectedSecondaryCauseOptions();

            _isLoading = false;
            StateHasChanged(); 
        }
    }

    private async Task OnSubmit()
    {
        if (_editContext.Validate())
        {
            await OnValidSubmit();
        }
    }

    private async Task OnValidSubmit()
    {
        // Update the eligibility check
        var eligibilityCheck = await GetEligibilityCheck();
        var updated = eligibilityCheck with
        {
            SecondaryCauses = [.. Model.SecondaryCauseOptions],
        };

        await protectedSessionStorage.SetAsync(SessionConstants.EligibilityCheck, updated);

        // Go to the next page, which is always the summary
        navigationManager.NavigateTo(NextPage.Url);
    }

    private async Task<EligibilityCheckDto> GetEligibilityCheck()
    {
        var data = await protectedSessionStorage.GetAsync<EligibilityCheckDto>(SessionConstants.EligibilityCheck);
        if (data.Success && data.Value != null)
        {
            return data.Value;
        }

        logger.LogDebug("Eligibility Check was not found in the protected storage.");
        return new();
    }

    /// <summary>
    /// Set up the selected secondary causes options (string, bool dictionary)
    /// </summary>
    private void UpdateSelectedSecondaryCauseOptions()
    {
        SelectedSecondaryCauseOptions = SecondaryCauseOptions.ToDictionary(o => o.Id.ToString("N"), o => Model.SecondaryCauseOptions.Contains(o.Id), StringComparer.Ordinal);
    }

    private void OnSecondaryCauseChanged(bool isChecked, Guid floodProblemId)
    {
        // update the model
        if (isChecked && !Model.SecondaryCauseOptions.Contains(floodProblemId))
            Model.SecondaryCauseOptions.Add(floodProblemId);
        else if (!isChecked)
            Model.SecondaryCauseOptions.Remove(floodProblemId);
    }

}

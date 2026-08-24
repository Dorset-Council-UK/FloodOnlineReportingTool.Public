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

public partial class Cause(
    ILogger<Cause> logger,
    ICommonRepository commonRepository,
    ProtectedSessionStorage protectedSessionStorage,
    NavigationManager navigationManager
) : IAsyncDisposable
{
    private Models.FloodReport.Create.FloodCause Model { get; set; } = default!;

    [SupplyParameterFromQuery]
    private bool FromSummary { get; set; }
    private PageInfo? PreviousPage;

    private EditContext _editContext = default!;
    private readonly CancellationTokenSource _cts = new();
    private bool _isLoading = true;
    private IList<FloodProblem> CauseOptions { get; set; } = [];
    private Dictionary<string, bool> SelectedCauseOptions = [];

    protected override async Task OnInitializedAsync()
    {
        if (Model is null)
        {
            // Setup model and edit context
            Model ??= new();
            _editContext = new(Model);
            _editContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
        }

        CauseOptions = await commonRepository.GetFloodProblemsByCategory(FloodProblemCategory.PrimaryCause, _cts.Token);
        UpdateSelectedCauseOptions();
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

            Model.CauseOptions = [..eligibilityCheck.Causes];
            UpdateSelectedCauseOptions();

            PreviousPage = FromSummary
                ? FloodReportCreatePages.Summary
                : eligibilityCheck.OnGoing ? FloodReportCreatePages.FloodStarted : FloodReportCreatePages.FloodDuration;

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

        var selectedOptions = SelectedCauseOptions.Where(o => o.Value).Select(o => Guid.Parse(o.Key));
        var hasRainwaterCause = selectedOptions.Contains(PrimaryCauseIds.RainwaterFlowingOverTheGround);

        // We need to remove any run off options as it has not been selected
        IList<Guid> secondaryCauses = !hasRainwaterCause ? [] : eligibilityCheck.SecondaryCauses;

        var updated = eligibilityCheck with
        {
            Causes = [.. selectedOptions],
            SecondaryCauses = secondaryCauses,
        };

        await protectedSessionStorage.SetAsync(SessionConstants.EligibilityCheck, updated);

        // Go to the next page, summary or secondary cause
        PageInfo? nextPage = null;
        if (FromSummary)
        {
            // The summary page takes priority
            nextPage = FloodReportCreatePages.Summary;
        }
        else if (hasRainwaterCause)
        {
            nextPage = FloodReportCreatePages.SecondaryCause;
        }
        else
        {
            // The next page is summary anyway
            nextPage = FloodReportCreatePages.Summary;
        }

        navigationManager.NavigateTo(nextPage.Url);
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
    /// Set up the selected cause options (string, bool dictionary)
    /// </summary>
    private void UpdateSelectedCauseOptions()
    {
        SelectedCauseOptions = CauseOptions.ToDictionary(o => o.Id.ToString("N"), o => Model.CauseOptions.Contains(o.Id), StringComparer.Ordinal);
    }

    private void OnCauseChanged(bool isChecked, Guid floodProblemId)
    {
        // update the model
        if (isChecked && !Model.CauseOptions.Contains(floodProblemId))
            Model.CauseOptions.Add(floodProblemId);
        else if (!isChecked)
            Model.CauseOptions.Remove(floodProblemId);
    }

}

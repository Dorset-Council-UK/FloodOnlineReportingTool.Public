using FloodOnlineReportingTool.Database.Models.Flood;
using FloodOnlineReportingTool.Database.Models.Flood.FloodProblemIds;
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
public partial class Entry(
    ILogger<Entry> logger,
    ICommonRepository commonRepository,
    ProtectedSessionStorage protectedSessionStorage,
    NavigationManager navigationManager
) : IAsyncDisposable
{
    // Page order properties
    public string Title { get; set; } = InvestigationPages.InternalHow.Title;

    [SupplyParameterFromQuery]
    private bool FromSummary { get; set; }
    private PageInfo NextPage => FromSummary
        ? InvestigationPages.Summary
        : InvestigationPages.InternalWhen;
    private static PageInfo PreviousPage => InvestigationPages.Vehicles;

    private Models.FloodReport.Investigation.Entry Model { get; set; } = default!;

    private EditContext _editContext = default!;
    private readonly CancellationTokenSource _cts = new();
    private bool _isLoading = true;
    private IList<FloodProblem> FloodEntryOptions = [];
    private Dictionary<string, bool> SelectedFloodEntryOptions = [];

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

        FloodEntryOptions = await commonRepository.GetFloodProblemsByCategory(FloodProblemCategory.Entry, _cts.Token);
        UpdateSelectedFloodEntryOptions();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Set any previously entered data
            var investigation = await GetInvestigation();
            Model.EntryOptions = [.. investigation.Entries];
            Model.WaterEnteredOther = investigation.WaterEnteredOther;
            UpdateSelectedFloodEntryOptions();

            _isLoading = false;
            StateHasChanged();
        }
    }

    private async Task OnValidSubmit()
    {
        var investigation = await GetInvestigation();
        var updatedInvestigation = investigation with
        {
            Entries = Model.EntryOptions,
            WaterEnteredOther = Model.EntryOptions.Contains(FloodEntryIds.Other) ? Model.WaterEnteredOther : null,
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
    /// Set up the selected flood entry options (string, bool dictionary)
    /// </summary>
    private void UpdateSelectedFloodEntryOptions()
    {
        SelectedFloodEntryOptions = FloodEntryOptions.ToDictionary(o => o.Id.ToString("N"), o => Model.EntryOptions.Contains(o.Id), StringComparer.Ordinal);
    }

    private void OnFloodEntryChanged(bool isChecked, Guid floodEntryId)
    {
        // update the model
        if (isChecked && !Model.EntryOptions.Contains(floodEntryId))
            Model.EntryOptions.Add(floodEntryId);
        else if (!isChecked)
            Model.EntryOptions.Remove(floodEntryId);
    }

}

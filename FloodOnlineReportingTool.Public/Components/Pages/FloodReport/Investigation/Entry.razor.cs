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
) : IPageOrder, IAsyncDisposable
{
    // Page order properties
    public string Title { get; set; } = InvestigationPages.InternalHow.Title;
    public IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [
        GeneralPages.Home.ToGdsBreadcrumb(),
        FloodReportPages.Overview.ToGdsBreadcrumb(),
        InvestigationPages.Vehicles.ToGdsBreadcrumb(),
    ];

    [SupplyParameterFromQuery]
    private bool FromSummary { get; set; }

    private Models.FloodReport.Investigation.Entry Model { get; set; } = default!;

    private EditContext _editContext = default!;
    private readonly CancellationTokenSource _cts = new();
    private bool _isLoading = true;

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

    protected override void OnInitialized()
    {
        // Setup model and edit context
        Model ??= new();
        _editContext = new(Model);
        _editContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Set any previously entered data
            var investigation = await GetInvestigation();
            Model.EntryOptions = await CreateEntryOptions(investigation.Entries);
            Model.WaterEnteredOther = investigation.WaterEnteredOther;

            _isLoading = false;
            StateHasChanged();

            
        }
    }

    private async Task OnValidSubmit()
    {
        var otherEntrySelected = Model.EntryOptions.Any(option => option.Selected && option.Value.Equals(FloodEntryIds.Other));
        var hasOther = Model.EntryOptions.Any(option => option.Value.Equals(FloodEntryIds.Other));
        var investigation = await GetInvestigation();
        var updatedInvestigation = investigation with
        {
            Entries = [.. Model.EntryOptions.Where(o => o.Selected).Select(o => o.Value)],
            WaterEnteredOther = otherEntrySelected ? Model.WaterEnteredOther : null,
        };
        await protectedSessionStorage.SetAsync(SessionConstants.Investigation, updatedInvestigation);

        // Go to the next page or back to the summary
        var nextPage = FromSummary ? InvestigationPages.Summary : InvestigationPages.InternalWhen;
        navigationManager.NavigateTo(nextPage.Url);
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

    private async Task<IReadOnlyCollection<GdsOptionItem<Guid>>> CreateEntryOptions(IList<Guid> selectedValues)
    {
        var floodProblems = await commonRepository.GetFloodProblemsByCategory(FloodProblemCategory.Entry, _cts.Token);
        return [.. floodProblems.Select(o => CreateOption(o, "water-entry", selectedValues))];
    }

    private static GdsOptionItem<Guid> CreateOption(FloodProblem floodProblem, string idPrefix, IList<Guid> selectedValues)
    {
        var id = $"{idPrefix}-{floodProblem.Id}".AsSpan();
        var label = floodProblem.TypeName.AsSpan();
        var selected = selectedValues.Contains(floodProblem.Id);
        var isExclusive = floodProblem.Id == FloodEntryIds.NotSure;

        return new GdsOptionItem<Guid>(id, label, floodProblem.Id, selected, isExclusive);
    }
}

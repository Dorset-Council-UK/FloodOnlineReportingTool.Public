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

public partial class FloodSecondarySource(
    ILogger<FloodSecondarySource> logger,
    ICommonRepository commonRepository,
    ProtectedSessionStorage protectedSessionStorage,
    NavigationManager navigationManager
) : IPageOrder, IAsyncDisposable
{
    // Page order properties
    public string Title { get; set; } = FloodReportCreatePages.FloodSecondarySource.Title;
    public IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [
        GeneralPages.Home.ToGdsBreadcrumb(),
        FloodReportPages.Home.ToGdsBreadcrumb(),
        FloodReportCreatePages.FloodSource.ToGdsBreadcrumb()
    ];

    private Models.FloodReport.Create.FloodSecondarySource Model { get; set; } = default!;
    
    [SupplyParameterFromQuery]
    private bool FromSummary { get; set; }
    private PageInfo NextPage => FloodReportCreatePages.Summary;
    private PageInfo PreviousPage => FloodReportCreatePages.FloodSource;

    private EditContext _editContext = default!;
    private readonly CancellationTokenSource _cts = new();
    private bool _isLoading = true;

    protected override void OnInitialized()
    {
        // Setup model and edit context
        Model ??= new();
        _editContext = new(Model);
        _editContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
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

            Model.FloodSecondarySourceOptions = await CreateFloodSourceOptions(eligibilityCheck.SecondarySources);

            _isLoading = false;
            StateHasChanged();

            
        }
    }

    private async Task OnValidSubmit()
    {
        // Update the eligibility check
        var eligibilityCheck = await GetEligibilityCheck();
        var updated = eligibilityCheck with
        {
            SecondarySources = [.. Model.FloodSecondarySourceOptions.Where(o => o.Selected).Select(o => o.Value)],
        };

        await protectedSessionStorage.SetAsync(SessionConstants.EligibilityCheck, updated);

        // Go to the next page, which is always the summary
        navigationManager.NavigateTo(FloodReportCreatePages.Summary.Url);
    }

    private async Task<EligibilityCheckDto> GetEligibilityCheck()
    {
        var data = await protectedSessionStorage.GetAsync<EligibilityCheckDto>(SessionConstants.EligibilityCheck);
        if (data.Success)
        {
            if (data.Value != null)
            {
                return data.Value;
            }
        }

        logger.LogDebug("Eligibility Check was not found in the protected storage.");
        return new();
    }

    private async Task<IReadOnlyCollection<GdsOptionItem<Guid>>> CreateFloodSourceOptions(IList<Guid> selectedValues)
    {
        const string idPrefix = "flood--secondary-source";
        var floodProblems = await commonRepository.GetFloodProblemsByCategory(FloodProblemCategory.SecondaryCause, _cts.Token);
        return [.. floodProblems.Select((o, idx) => CreateOption(o, idPrefix, selectedValues))];
    }

    private static GdsOptionItem<Guid> CreateOption(FloodProblem floodProblem, string idPrefix, IList<Guid> selectedValues)
    {
        var id = $"{idPrefix}-{floodProblem.Id}".AsSpan();
        var label = floodProblem.TypeName.AsSpan();
        var selected = selectedValues.Contains(floodProblem.Id);
        var isExclusive = floodProblem.Id == SecondaryCauseIds.NotSure;

        return new GdsOptionItem<Guid>(id, label, floodProblem.Id, selected, isExclusive);
    }

    private Task OnPreviousPage()
    {
        // Go to previous page or return to summary
        var previousPage = FromSummary ? FloodReportCreatePages.Summary : PreviousPage;
        navigationManager.NavigateTo(previousPage.Url);

        return Task.CompletedTask;
    }
}


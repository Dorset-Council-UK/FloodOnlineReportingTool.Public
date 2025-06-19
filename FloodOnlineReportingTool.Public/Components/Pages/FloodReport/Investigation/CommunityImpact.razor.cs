using FloodOnlineReportingTool.DataAccess.Models;
using FloodOnlineReportingTool.DataAccess.Repositories;
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
    NavigationManager navigationManager,
    IGdsJsInterop gdsJs
) : IPageOrder, IAsyncDisposable
{
    // Page order properties
    public string Title { get; set; } = InvestigationPages.CommunityImpact.Title;
    public IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [
        GeneralPages.Home.ToGdsBreadcrumb(),
        FloodReportPages.Overview.ToGdsBreadcrumb(),
        InvestigationPages.PeakDepth.ToGdsBreadcrumb(),
    ];

    [SupplyParameterFromQuery]
    private bool FromSummary { get; set; }

    private Models.FloodReport.Investigation.CommunityImpact Model { get; set; } = default!;

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
            var options = await CreateCommunityImpactOptions(investigation.CommunityImpacts);
            Model.CommunityImpactOptions = [.. options];

            _isLoading = false;
            StateHasChanged();

            await gdsJs.InitGds(_cts.Token);
        }
    }

    private async Task OnValidSubmit()
    {
        var investigation = await GetInvestigation();
        var updatedInvestigation = investigation with
        {
            CommunityImpacts = [.. Model.CommunityImpactOptions.Where(o => o.Selected).Select(o => o.Value)],
        };
        await protectedSessionStorage.SetAsync(SessionConstants.Investigation, updatedInvestigation);

        // Go to the next page or back to the summary
        var nextPage = FromSummary ? InvestigationPages.Summary : InvestigationPages.Blockages;
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

    private async Task<IList<GdsOptionItem<Guid>>> CreateCommunityImpactOptions(IList<Guid> selectedValues)
    {
        const string idPrefix = "community-impact";
        var floodProblems = await commonRepository.GetFloodImpactsByCategory(FloodImpactCategory.CommunityImpact, _cts.Token);
        return [.. floodProblems.Select(o => CreateOption(o, idPrefix, selectedValues))];
    }

    private static GdsOptionItem<Guid> CreateOption(FloodImpact floodImpact, string idPrefix, IList<Guid> selectedValues)
    {
        var id = $"{idPrefix}-{floodImpact.Id}".AsSpan();
        var label = floodImpact.TypeName.AsSpan();
        var selected = selectedValues.Contains(floodImpact.Id);
        var isExclusive = floodImpact.Id == FloodImpactIds.CommunityImpactNotSure;

        return new GdsOptionItem<Guid>(id, label, floodImpact.Id, selected, isExclusive);
    }
}
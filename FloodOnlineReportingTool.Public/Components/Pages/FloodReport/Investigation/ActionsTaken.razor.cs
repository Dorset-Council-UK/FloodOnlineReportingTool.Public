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
    NavigationManager navigationManager,
    IGdsJsInterop gdsJs
) : IPageOrder, IAsyncDisposable
{
    // Page order properties
    public string Title { get; set; } = InvestigationPages.ActionsTaken.Title;
    public IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [
        GeneralPages.Home.ToGdsBreadcrumb(),
        FloodReportPages.Overview.ToGdsBreadcrumb(),
        InvestigationPages.Blockages.ToGdsBreadcrumb(),
    ];

    [SupplyParameterFromQuery]
    private bool FromSummary { get; set; }

    private Models.FloodReport.Investigation.ActionsTaken Model { get; set; } = default!;

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
            Model.ActionsTakenOptions = await CreateActionsTakenOptions(investigation.ActionsTaken);
            Model.OtherAction = investigation.OtherAction;

            _isLoading = false;
            StateHasChanged();

            //await gdsJs.InitGds(_cts.Token);
        }
    }

    private async Task OnValidSubmit()
    {
        var selectedActions = Model.ActionsTakenOptions
            .Where(o => o.Selected)
            .Select(o => o.Value)
            .ToList();

        var investigation = await GetInvestigation();
        var updatedInvestigation = investigation with
        {
            ActionsTaken = selectedActions,
            OtherAction = selectedActions.Contains(FloodMitigationIds.OtherAction) ? Model.OtherAction : null,
        };
        await protectedSessionStorage.SetAsync(SessionConstants.Investigation, updatedInvestigation);

        // Go to the next page or back to the summary
        var nextPage = FromSummary ? InvestigationPages.Summary : InvestigationPages.HelpReceived;
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

    private async Task<IReadOnlyCollection<GdsOptionItem<Guid>>> CreateActionsTakenOptions(IList<Guid> selectedValues)
    {
        const string idPrefix = "action-taken";
        var floodMitigations = await commonRepository.GetFloodMitigationsByCategory(FloodMitigationCategory.ActionsTaken, _cts.Token);
        return [.. floodMitigations.Select(o => CreateOption(o, idPrefix, selectedValues))];
    }

    private static GdsOptionItem<Guid> CreateOption(FloodMitigation floodMitigation, string idPrefix, IList<Guid> selectedValues)
    {
        var id = $"{idPrefix}-{floodMitigation.Id}".AsSpan();
        var label = floodMitigation.TypeName.AsSpan();
        var selected = selectedValues.Contains(floodMitigation.Id);
        var isExclusive = floodMitigation.Id == FloodMitigationIds.NoActionTaken;

        return new GdsOptionItem<Guid>(id, label, floodMitigation.Id, selected, isExclusive);
    }
}

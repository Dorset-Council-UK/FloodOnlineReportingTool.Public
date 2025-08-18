using FloodOnlineReportingTool.Database.Models;
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
    NavigationManager navigationManager,
    IGdsJsInterop gdsJs
) : IPageOrder, IAsyncDisposable
{
    // Page order properties
    public string Title { get; set; } = InvestigationPages.HelpReceived.Title;
    public IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [
        GeneralPages.Home.ToGdsBreadcrumb(),
        FloodReportPages.Overview.ToGdsBreadcrumb(),
        InvestigationPages.ActionsTaken.ToGdsBreadcrumb(),
    ];

    [SupplyParameterFromQuery]
    private bool FromSummary { get; set; }

    private Models.FloodReport.Investigation.HelpReceived Model { get; set; } = default!;

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
            var options = await CreateHelpReceivedOptions(investigation.HelpReceived);
            Model.HelpReceivedOptions = [.. options];

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
            HelpReceived = [.. Model.HelpReceivedOptions.Where(o => o.Selected).Select(o => o.Value)],
        };
        await protectedSessionStorage.SetAsync(SessionConstants.Investigation, updatedInvestigation);

        // Go to the next page or back to the summary
        var nextPage = FromSummary ? InvestigationPages.Summary : InvestigationPages.Warnings;
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

    private async Task<IList<GdsOptionItem<Guid>>> CreateHelpReceivedOptions(IList<Guid> selectedValues)
    {
        const string idPrefix = "help-received";
        var floodMitigations = await commonRepository.GetFloodMitigationsByCategory(FloodMitigationCategory.HelpReceived, _cts.Token);
        return [.. floodMitigations.Select(o => CreateOption(o, idPrefix, selectedValues))];
    }

    private static GdsOptionItem<Guid> CreateOption(FloodMitigation floodMitigation, string idPrefix, IList<Guid> selectedValues)
    {
        var id = $"{idPrefix}-{floodMitigation.Id}".AsSpan();
        var label = floodMitigation.TypeName.AsSpan();
        var selected = selectedValues.Contains(floodMitigation.Id);
        var isExclusive = floodMitigation.Id == FloodMitigationIds.NoHelp;

        return new GdsOptionItem<Guid>(id, label, floodMitigation.Id, selected, isExclusive);
    }
}

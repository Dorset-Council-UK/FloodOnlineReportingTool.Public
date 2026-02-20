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
public partial class WarningSources(
    ILogger<WarningSources> logger,
    ICommonRepository commonRepository,
    ProtectedSessionStorage protectedSessionStorage,
    NavigationManager navigationManager
) : IAsyncDisposable
{
    // Page order properties
    public string Title { get; set; } = InvestigationPages.WarningSources.Title;

    [SupplyParameterFromQuery]
    private bool FromSummary { get; set; }
    private static PageInfo PreviousPage => InvestigationPages.Warnings;

    private Models.FloodReport.Investigation.WarningSources Model { get; set; } = default!;

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
            Model.WarningSourceOptions = await CreateWarningSourceOptions(investigation.WarningSources);
            Model.WarningOther = investigation.WarningSourceOther;

            _isLoading = false;
            StateHasChanged(); 
        }
    }

    private async Task OnValidSubmit()
    {
        var isOtherWarningSelected = Model.WarningSourceOptions.Any(o => o.Selected && o.Value == FloodMitigationIds.OtherWarning);
        var investigation = await GetInvestigation();
        var updatedInvestigation = investigation with
        {
            WarningSources = [.. Model.WarningSourceOptions.Where(o => o.Selected).Select(o => o.Value)],
            WarningSourceOther = isOtherWarningSelected ? Model.WarningOther : null,
        };
        await protectedSessionStorage.SetAsync(SessionConstants.Investigation, updatedInvestigation);

        // Go to the next page or back to the summary
        navigationManager.NavigateTo(GetNextPage().Url);
    }

    private PageInfo GetNextPage()
    {
        if (FromSummary)
        {
            return InvestigationPages.Summary;
        }

        var isFloodLineWarningSelected = Model.WarningSourceOptions.Any(o => o.Selected && o.Value == FloodMitigationIds.FloodlineWarning);
        return isFloodLineWarningSelected ? InvestigationPages.Floodline : InvestigationPages.History;
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

    private async Task<IReadOnlyCollection<GdsOptionItem<Guid>>> CreateWarningSourceOptions(IList<Guid> selectedValues)
    {
        const string idPrefix = "warning-source";
        var floodMitigations = await commonRepository.GetFloodMitigationsByCategory(FloodMitigationCategory.WarningSource, _cts.Token);
        return [.. floodMitigations.Select(o => CreateOption(o, idPrefix, selectedValues))];
    }

    private static GdsOptionItem<Guid> CreateOption(FloodMitigation floodMitigation, string idPrefix, IList<Guid> selectedValues)
    {
        var id = $"{idPrefix}-{floodMitigation.Id}".AsSpan();
        var label = floodMitigation.TypeName.AsSpan();
        var selected = selectedValues.Contains(floodMitigation.Id);
        var isExclusive = floodMitigation.Id == FloodMitigationIds.NoWarning;

        return new GdsOptionItem<Guid>(id, label, floodMitigation.Id, selected, isExclusive);
    }
}

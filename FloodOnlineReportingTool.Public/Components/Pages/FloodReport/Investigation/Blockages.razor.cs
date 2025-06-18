using FloodOnlineReportingTool.DataAccess.Models;
using FloodOnlineReportingTool.DataAccess.Repositories;
using FloodOnlineReportingTool.GdsComponents;
using FloodOnlineReportingTool.Public.Models;
using FloodOnlineReportingTool.Public.Models.Order;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport.Investigation;

[Authorize]
public partial class Blockages(
    ILogger<Blockages> logger,
    ICommonRepository commonRepository,
    ProtectedSessionStorage protectedSessionStorage,
    NavigationManager navigationManager,
    IJSRuntime JS
) : IPageOrder, IAsyncDisposable
{
    // Page order properties
    public string Title { get; set; } = InvestigationPages.Blockages.Title;
    public IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [
        GeneralPages.Home.ToGdsBreadcrumb(),
        FloodReportPages.Overview.ToGdsBreadcrumb(),
        InvestigationPages.CommunityImpact.ToGdsBreadcrumb(),
    ];

    [SupplyParameterFromQuery]
    private bool FromSummary { get; set; }

    private Models.FloodReport.Investigation.Blockages Model { get; set; } = default!;

    private EditContext _editContext = default!;
    private readonly CancellationTokenSource _cts = new();
    private bool _isLoading = true;
    private IReadOnlyCollection<GdsOptionItem<Guid>> _blockageOptions = [];

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
            Model.HasKnownProblemsId = investigation.HasKnownProblems == true ? RecordStatusIds.Yes : null;
            Model.KnownProblemDetails = investigation.KnownProblemDetails;

            _blockageOptions = await CreateBlockageOptions();

            _isLoading = false;
            StateHasChanged();

            await JS.InvokeVoidAsync("window.initGDS", _cts.Token);
        }
    }

    private async Task OnValidSubmit()
    {
        bool? hasKnown = null;
        if (Model.HasKnownProblemsId == RecordStatusIds.Yes) hasKnown = true;
        if (Model.HasKnownProblemsId == RecordStatusIds.No) hasKnown = false;

        var investigation = await GetInvestigation();
        var updatedInvestigation = investigation with
        {
            HasKnownProblems = hasKnown,
            KnownProblemDetails = Model.KnownProblemDetails,
        };
        await protectedSessionStorage.SetAsync(SessionConstants.Investigation, updatedInvestigation);

        // Go to the next page or back to the summary
        var nextPage = FromSummary ? InvestigationPages.Summary : InvestigationPages.ActionsTaken;
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

    private async Task<IReadOnlyCollection<GdsOptionItem<Guid>>> CreateBlockageOptions()
    {
        const string idPrefix = "blockage-known";
        var recordStatuses = await commonRepository.GetRecordStatusesByCategory(RecordStatusCategory.General, _cts.Token);
        var withoutNotSure = recordStatuses.Where(o => o.Id != RecordStatusIds.NotSure).ToList();
        return [.. withoutNotSure.Select(o => CreateOption(o, idPrefix, Model.HasKnownProblemsId))];
    }

    private static GdsOptionItem<Guid> CreateOption(RecordStatus recordStatus, string idPrefix, Guid? selectedValue)
    {
        var id = $"{idPrefix}-{recordStatus.Id}".AsSpan();
        var label = recordStatus.Text.AsSpan();
        var selected = recordStatus.Id == selectedValue;
        var isExclusive = recordStatus.Id == RecordStatusIds.NotSure;

        return new GdsOptionItem<Guid>(id, label, recordStatus.Id, selected, isExclusive);
    }
}

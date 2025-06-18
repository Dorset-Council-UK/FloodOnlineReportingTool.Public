using FloodOnlineReportingTool.DataAccess.Models;
using FloodOnlineReportingTool.DataAccess.Repositories;
using FloodOnlineReportingTool.GdsComponents;
using FloodOnlineReportingTool.Public.Models;
using FloodOnlineReportingTool.Public.Models.Order;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;
using System.Globalization;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport.Investigation;

[Authorize]
public partial class PeakDepth(
    ILogger<PeakDepth> logger,
    ICommonRepository commonRepository,
    IEligibilityCheckRepository eligibilityCheckRepository,
    ProtectedSessionStorage protectedSessionStorage,
    NavigationManager navigationManager,
    IJSRuntime JS
) : IPageOrder, IAsyncDisposable
{
    // Page order properties
    public string Title { get; set; } = InvestigationPages.PeakDepth.Title;
    public IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [
        GeneralPages.Home.ToGdsBreadcrumb(),
        FloodReportPages.Overview.ToGdsBreadcrumb(),
    ];

    [CascadingParameter]
    public Task<AuthenticationState>? AuthenticationState { get; set; }

    [SupplyParameterFromQuery]
    private bool FromSummary { get; set; }

    private Models.FloodReport.Investigation.PeakDepth Model { get; set; } = default!;

    private EditContext _editContext = default!;
    private readonly CancellationTokenSource _cts = new();
    private bool _isLoading = true;
    private IReadOnlyCollection<GdsOptionItem<Guid>> _peakDepthKnownOptions = [];

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

    private async Task<IReadOnlyCollection<GdsBreadcrumb>> GetBreadcrumbs()
    {
        var userId = await AuthenticationState.IdentityUserId() ?? Guid.Empty;
        var eligibilityCheck = await eligibilityCheckRepository.ReportedByUser(userId, _cts.Token);

        var pageInfo = eligibilityCheck?.IsInternal() == true
            ? InvestigationPages.InternalWhen
            : InvestigationPages.Vehicles;

        return Breadcrumbs.Append(pageInfo.ToGdsBreadcrumb()).ToList();
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
            Breadcrumbs = await GetBreadcrumbs();

            // Set any previously entered data
            var investigation = await GetInvestigation();
            Model.IsPeakDepthKnownId = investigation.IsPeakDepthKnownId;
            Model.InsideCentimetresText = investigation.PeakInsideCentimetres?.ToString(CultureInfo.CurrentCulture);
            Model.OutsideCentimetresText = investigation.PeakOutsideCentimetres?.ToString(CultureInfo.CurrentCulture);

            _peakDepthKnownOptions = await CreatePeakDepthKnownOptions();

            _isLoading = false;
            StateHasChanged();

            await JS.InvokeVoidAsync("window.initGDS", _cts.Token);
        }
    }

    private async Task OnValidSubmit()
    {
        var isKnown = Model.IsPeakDepthKnownId == RecordStatusIds.Yes;
        var investigation = await GetInvestigation();
        var updatedInvestigation = investigation with
        {
            IsPeakDepthKnownId = Model.IsPeakDepthKnownId,
            PeakInsideCentimetres = isKnown ? Model.InsideCentimetres : null,
            PeakOutsideCentimetres = isKnown ? Model.OutsideCentimetres : null,
        };
        await protectedSessionStorage.SetAsync(SessionConstants.Investigation, updatedInvestigation);

        // Go to the next page or back to the summary
        var nextPage = FromSummary ? InvestigationPages.Summary : InvestigationPages.CommunityImpact;
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

    private async Task<IReadOnlyCollection<GdsOptionItem<Guid>>> CreatePeakDepthKnownOptions()
    {
        var recordStatuses = await commonRepository.GetRecordStatusesByCategory(RecordStatusCategory.General, _cts.Token);
        var withoutNotSure = recordStatuses.Where(o => o.Id != RecordStatusIds.NotSure).ToList();
        return [.. withoutNotSure.Select(o => CreateOption(o, "depth-known", Model.IsPeakDepthKnownId))];
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

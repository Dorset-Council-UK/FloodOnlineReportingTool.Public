using FloodOnlineReportingTool.Database.Models.Eligibility;
using FloodOnlineReportingTool.Database.Models.Investigate;
using FloodOnlineReportingTool.Database.Models.Status;
using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.Public.Models;
using FloodOnlineReportingTool.Public.Models.Order;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Globalization;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport.Investigation;

[Authorize]
public partial class Vehicles(
    ILogger<Vehicles> logger,
    ICommonRepository commonRepository,
    IEligibilityCheckRepository eligibilityCheckRepository,
    ProtectedSessionStorage protectedSessionStorage,
    NavigationManager navigationManager
) : IPageOrder, IAsyncDisposable
{
    // Page order properties
    public string Title { get; set; } = InvestigationPages.Vehicles.Title;
    public IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [
        GeneralPages.Home.ToGdsBreadcrumb(),
        FloodReportPages.Overview.ToGdsBreadcrumb(),
        InvestigationPages.Destination.ToGdsBreadcrumb(),
    ];

    [CascadingParameter]
    public Task<AuthenticationState>? AuthenticationState { get; set; }

    [SupplyParameterFromQuery]
    private bool FromSummary { get; set; }

    private Models.FloodReport.Investigation.Vehicles Model { get; set; } = default!;

    private EditContext _editContext = default!;
    private readonly CancellationTokenSource _cts = new();
    private bool _isLoading = true;
    private IReadOnlyCollection<GdsOptionItem<Guid>> _wereVehiclesDamagedOptions = [];

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
            Model.WereVehiclesDamagedId = investigation.WereVehiclesDamagedId;
            Model.NumberOfVehiclesDamagedNumber = investigation.NumberOfVehiclesDamaged;
            Model.NumberOfVehiclesDamagedText = investigation.NumberOfVehiclesDamaged?.ToString(CultureInfo.CurrentCulture);

            _wereVehiclesDamagedOptions = await CreateVehiclesDamagedOptions();

            _isLoading = false;
            StateHasChanged();

            
        }
    }

    private async Task OnValidSubmit()
    {
        var investigation = await GetInvestigation();
        var updatedInvestigation = investigation with
        {
            WereVehiclesDamagedId = Model.WereVehiclesDamagedId,
            NumberOfVehiclesDamaged = Model.WereVehiclesDamagedId == RecordStatusIds.Yes ? (byte?)Model.NumberOfVehiclesDamagedNumber : null,
        };
        await protectedSessionStorage.SetAsync(SessionConstants.Investigation, updatedInvestigation);

        // Go to the next page or back to the summary
        var nextPage = await GetNextPage();
        navigationManager.NavigateTo(nextPage.Url);
    }

    private async Task<PageInfo> GetNextPage()
    {
        if (FromSummary)
        {
            return InvestigationPages.Summary;
        }

        var userId = await AuthenticationState.IdentityUserId() ?? Guid.Empty;
        var eligibilityCheck = await eligibilityCheckRepository.ReportedByUser(userId, _cts.Token);
        if (eligibilityCheck?.IsInternal() == true)
        {
            return InvestigationPages.InternalHow;
        }

        return InvestigationPages.PeakDepth;
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

    private async Task<IReadOnlyCollection<GdsOptionItem<Guid>>> CreateVehiclesDamagedOptions()
    {
        var floodProblems = await commonRepository.GetRecordStatusesByCategory(RecordStatusCategory.General, _cts.Token);
        return [.. floodProblems.Select(o => CreateOption(o, "vehicles-damaged", Model.WereVehiclesDamagedId))];
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

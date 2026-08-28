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
using System.Security.Claims;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport.Investigation;

[Authorize]
public partial class Vehicles(
    ILogger<Vehicles> logger,
    ICommonRepository commonRepository,
    IEligibilityCheckRepository eligibilityCheckRepository,
    ProtectedSessionStorage protectedSessionStorage,
    NavigationManager navigationManager
) : IAsyncDisposable
{
    // Page order properties
    public string Title { get; set; } = InvestigationPages.Vehicles.Title;

    [CascadingParameter]
    public Task<AuthenticationState>? AuthenticationState { get; set; }

    [SupplyParameterFromQuery]
    private bool FromSummary { get; set; }
    private static PageInfo PreviousPage => InvestigationPages.Destination;

    private Models.FloodReport.Investigation.Vehicles Model { get; set; } = default!;

    private EditContext _editContext = default!;
    private readonly CancellationTokenSource _cts = new();
    private bool _isLoading = true;
    private IList<RecordStatus> _wereVehiclesDamagedOptions = [];

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

        _wereVehiclesDamagedOptions = await commonRepository.GetRecordStatusesByCategory(RecordStatusCategory.General, _cts.Token);
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

        bool isInternal = false;
        if (AuthenticationState is not null)
        {
            var authState = await AuthenticationState;
            var userId = authState.User.Oid;
            if (userId is not null)
            {
                var eligibilityCheck = await eligibilityCheckRepository.ReportedByUser(userId, _cts.Token);
                isInternal = eligibilityCheck?.IsInternal == true;
            }
        }

        return isInternal ? InvestigationPages.InternalHow : InvestigationPages.PeakDepth;
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

}

using FloodOnlineReportingTool.DataAccess.Models;
using FloodOnlineReportingTool.Public.Models;
using FloodOnlineReportingTool.Public.Models.Order;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport.Create;

public partial class FloodStarted(
    ILogger<FloodStarted> logger,
    ProtectedSessionStorage protectedSessionStorage,
    NavigationManager navigationManager,
    IGdsJsInterop gdsJs
) : IPageOrder, IAsyncDisposable
{
    // Page order properties
    public string Title { get; set; } = FloodReportCreatePages.FloodStarted.Title;
    public IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [
        GeneralPages.Home.ToGdsBreadcrumb(),
        FloodReportPages.Home.ToGdsBreadcrumb(),
        FloodReportCreatePages.Vulnerability.ToGdsBreadcrumb(),
    ];

    [SupplyParameterFromQuery]
    private bool FromSummary { get; set; }

    private Models.FloodReport.Create.FloodStarted Model { get; set; } = default!;

    private EditContext _editContext = default!;
    private readonly CancellationTokenSource _cts = new();
    private bool _isLoading = true;
    private readonly IReadOnlyCollection<GdsOptionItem<bool>> _floodOngoingOptions = [
        new ("flood-ongoing-yes", "Yes", value: true),
        new ("flood-ongoing-no", "No", value: false),
    ];

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
            var eligibilityCheck = await GetEligibilityCheck();
            var impactStart = eligibilityCheck.ImpactStart;
            if (impactStart.HasValue)
            {
                Model.StartDate = new GdsDate(impactStart.Value);
                Model.IsFloodOngoing = eligibilityCheck.OnGoing;
            }

            _isLoading = false;
            StateHasChanged();

            await gdsJs.InitGds(_cts.Token);
        }
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
        return new EligibilityCheckDto();
    }

    private async Task OnValidSubmit()
    {
        var isOnGoing = Model.IsFloodOngoing == true;

        var eligibilityCheck = await GetEligibilityCheck();
        var updated = eligibilityCheck with
        {
            ImpactStart = Model.StartDate.DateUtc,
            OnGoing = isOnGoing,
            ImpactDuration = isOnGoing ? 0 : eligibilityCheck.ImpactDuration,
        };

        await protectedSessionStorage.SetAsync(SessionConstants.EligibilityCheck, updated);

        // Go to the next page or back to the summary
        navigationManager.NavigateTo(NextPage(isOnGoing).Url);
    }

    private PageInfo NextPage(bool isOnGoing)
    {
        if (FromSummary)
        {
            return FloodReportCreatePages.Summary;
        }

        if (isOnGoing)
        {
            return FloodReportCreatePages.FloodSource;
        }

        return FloodReportCreatePages.FloodDuration;
    }

}

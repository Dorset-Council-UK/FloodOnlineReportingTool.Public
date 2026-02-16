using FloodOnlineReportingTool.Database.Models.Eligibility;
using FloodOnlineReportingTool.Public.Models;
using FloodOnlineReportingTool.Public.Models.Order;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport.Create;

public partial class Index(
    ILogger<Index> logger,
    NavigationManager navigationManager,
    ProtectedSessionStorage protectedSessionStorage
) : IPageOrder, IAsyncDisposable
{
    // Page order properties
    public string Title { get; set; } = FloodReportCreatePages.Home.Title;
    public IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [];

    [SupplyParameterFromQuery]
    private bool FromSummary { get; set; }
    private PageInfo NextPage => Model.IsAddress == true
        ? FloodReportCreatePages.Postcode 
        : FloodReportCreatePages.Location;
    private static PageInfo PreviousPage => FloodReportPages.Home;

    private Models.FloodReport.Create.Index Model { get; set; } = default!;

    private EditContext editContext = default!;
    private readonly CancellationTokenSource _cts = new();
    private bool _isLoading = true;
    private IReadOnlyCollection<GdsOptionItem<bool>> _isAddressOptions = [
        new("is-address-yes", "Yes", value: true),
        new("is-address-no", "No", value: false),
    ];

    protected override void OnInitialized()
    {
        // Setup model and edit context
        Model ??= new();
        editContext = new(Model);
        editContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Set any previously entered data
            var eligibilityCheck = await GetEligibilityCheck();
            Model.IsAddress = eligibilityCheck.IsAddress;
            _isAddressOptions.Single(o => o.Value).Selected = true;

            Breadcrumbs = CreateBreadcrumbs();
            _isLoading = false;
            StateHasChanged();
        }
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

    private async Task OnSubmit()
    {
        if (editContext.Validate())
        {
            await OnValidSubmit();
        }
    }

    private async Task OnValidSubmit()
    {
        // Set the IsAddress so that location page knows if this is a postal search rather than location
        var eligibilityCheck = await GetEligibilityCheck();
        var updatedEligibilityCheck = eligibilityCheck with
        {
            IsAddress = Model.IsAddress,
            LocationDesc = null, //Always reset this at this point to avoid unexpected results
        };

        await protectedSessionStorage.SetAsync(SessionConstants.EligibilityCheck, updatedEligibilityCheck);

        // Go to the next page or pass back to the summary
        var nextPageUrl = NextPage.Url;
        if (FromSummary)
        {
            nextPageUrl += "?fromsummary=true";
        }
        navigationManager.NavigateTo(nextPageUrl);
    }

    private void OnPreviousPage()
    {
        navigationManager.NavigateTo(PreviousPage.Url);
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

        logger.LogWarning("Eligibility Check was not found in the protected storage.");
        return new EligibilityCheckDto();
    }

    private static IReadOnlyCollection<GdsBreadcrumb> CreateBreadcrumbs()
    {
        return
        [
            GeneralPages.Home.ToGdsBreadcrumb(),
            FloodReportPages.Home.ToGdsBreadcrumb(),
        ];
    }
}

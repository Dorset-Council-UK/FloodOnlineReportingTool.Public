using FloodOnlineReportingTool.Database.Models;
using FloodOnlineReportingTool.Public.Models;
using FloodOnlineReportingTool.Public.Models.FloodReport.Create;
using FloodOnlineReportingTool.Public.Models.Order;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Net;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport.Create;

public partial class Index(
    ILogger<Index> logger,
    NavigationManager navigationManager,
    ProtectedSessionStorage protectedSessionStorage,
    IGdsJsInterop gdsJs
) : IPageOrder, IAsyncDisposable
{
    // Page order properties
    public string Title { get; set; } = FloodReportCreatePages.Home.Title;
    public IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [
        GeneralPages.Home.ToGdsBreadcrumb(),
        FloodReportPages.Home.ToGdsBreadcrumb(),
    ];

    [SupplyParameterFromQuery]
    private bool FromSummary { get; set; }

    private Models.FloodReport.Create.Index Model { get; set; } = default!;

    private EditContext editContext = default!;
    private readonly CancellationTokenSource _cts = new();
    private bool _isLoading = true;
    private IReadOnlyCollection<GdsOptionItem<bool>> _postcodeKnownOptions = [
        new("postcode-known-yes", "Yes", value: true),
        new("postcode-known-no", "No", value: false),
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
            var createExtraData = await GetCreateExtraData();
            Model.Postcode = createExtraData.Postcode;
            if (Model.Postcode != null)
            {
                Model.PostcodeKnown = true;
                _postcodeKnownOptions.Single(o => o.Value).Selected = true;
            }

            _isLoading = false;
            StateHasChanged();

            await gdsJs.InitGds(_cts.Token);
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

    private async Task OnValidSubmit()
    {
        // Save the postcode
        var createExtraData = await GetCreateExtraData();
        var updatedExtraData = createExtraData with
        {
            Postcode = Model.Postcode?.ToUpperInvariant(),
        };

        await protectedSessionStorage.SetAsync(SessionConstants.EligibilityCheck_ExtraData, updatedExtraData);

        if (Model.Postcode != null)
        {
            var eligibilityCheck = await GetEligibilityCheck();
            var updatedEligibilityCheck = eligibilityCheck with
            {
                IsAddress = true,
            };

            await protectedSessionStorage.SetAsync(SessionConstants.EligibilityCheck, updatedEligibilityCheck);
        }

        // Go to the next page or back to the summary
        var nextPage = GetNextPage();
        navigationManager.NavigateTo(nextPage.Url);
    }

    private PageInfo GetNextPage()
    {
        if (FromSummary)
        {
            return FloodReportCreatePages.Summary;
        }

        if (Model.PostcodeKnown == true)
        {
            return FloodReportCreatePages.Address;
        }

        return FloodReportCreatePages.Location;
    }

    private async Task<ExtraData> GetCreateExtraData()
    {
        var data = await protectedSessionStorage.GetAsync<ExtraData>(SessionConstants.EligibilityCheck_ExtraData);
        if (data.Success)
        {
            if (data.Value != null)
            {
                return data.Value;
            }
        }

        logger.LogWarning("Eligibility Check > Extra Data was not found in the protected storage.");
        return new();
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
}

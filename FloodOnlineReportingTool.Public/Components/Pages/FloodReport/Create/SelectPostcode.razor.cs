using FloodOnlineReportingTool.Public.Models;
using FloodOnlineReportingTool.Public.Models.FloodReport.Create;
using FloodOnlineReportingTool.Public.Models.Order;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport.Create;

public partial class SelectPostcode(
    ILogger<SelectPostcode> logger,
    NavigationManager navigationManager,
    ProtectedSessionStorage protectedSessionStorage
) : IPageOrder, IAsyncDisposable
{
    // Page order properties
    public string Title { get; set; } = FloodReportCreatePages.Postcode.Title;
    public IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [];

    [SupplyParameterFromQuery]
    private bool FromSummary { get; set; }
    private PageInfo PreviousPage => FloodReportCreatePages.Home;

    private Models.FloodReport.Create.SelectPostcode Model { get; set; } = default!;

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
        // Save the postcode
        var createExtraData = await GetCreateExtraData();
        var updatedExtraData = createExtraData with
        {
            Postcode = Model.Postcode?.ToUpperInvariant(),
        };

        await protectedSessionStorage.SetAsync(SessionConstants.EligibilityCheck_ExtraData, updatedExtraData);

        // Go to the next page or pass back to the summary
        navigationManager.NavigateTo(GetNextPage().Url);
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

    private void OnPreviousPage()
    {
        navigationManager.NavigateTo(PreviousPage.Url);
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

    private IReadOnlyCollection<GdsBreadcrumb> CreateBreadcrumbs()
    {
        return
        [
            GeneralPages.Home.ToGdsBreadcrumb(),
            FloodReportPages.Home.ToGdsBreadcrumb(),
            PreviousPage.ToGdsBreadcrumb(),
        ];
    }
}

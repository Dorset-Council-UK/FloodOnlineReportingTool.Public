using FloodOnlineReportingTool.Public.Models;
using FloodOnlineReportingTool.Public.Models.FloodReport.Create;
using FloodOnlineReportingTool.Public.Models.Order;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport.Create;

public partial class TemporarySelectPostcode(
    ILogger<TemporarySelectPostcode> logger,
    NavigationManager navigationManager,
    ProtectedSessionStorage protectedSessionStorage
) : IAsyncDisposable
{
    // Page order properties
    public string Title { get; set; } = FloodReportCreatePages.TemporaryPostcode.Title;

    [SupplyParameterFromQuery]
    private bool FromSummary { get; set; }
    private PageInfo NextPage => Model.PostcodeKnown == true
        ? FloodReportCreatePages.TemporaryAddress
        : FloodReportCreatePages.Vulnerability;
    private PageInfo PreviousPage => FloodReportCreatePages.FloodAreas;

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
            Model.Postcode = createExtraData.TemporaryPostcode;
            Model.PostcodeKnown = Model.Postcode != null;

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
        ExtraData updatedExtraData;
        if (Model.PostcodeKnown != null && (bool)Model.PostcodeKnown)
        {
            updatedExtraData = createExtraData with
            {
                TemporaryPostcode = Model.Postcode?.ToUpperInvariant(),
            };
        }
        else
        {
            updatedExtraData = createExtraData with
            {
                TemporaryPostcode = null,
            };
        }

        await protectedSessionStorage.SetAsync(SessionConstants.EligibilityCheck_ExtraData, updatedExtraData);

        // Go to the next page or pass back to the summary
        var nextPageUrl = NextPage.Url;
        if (FromSummary)
        {
            nextPageUrl += "?fromsummary=true";
        }
        navigationManager.NavigateTo(nextPageUrl);
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
}

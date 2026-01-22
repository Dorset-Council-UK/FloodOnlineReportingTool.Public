using FloodOnlineReportingTool.Database.Exceptions;
using FloodOnlineReportingTool.Database.Models.API;
using FloodOnlineReportingTool.Database.Models.Eligibility;
using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.Public.Models;
using FloodOnlineReportingTool.Public.Models.FloodReport.Create;
using FloodOnlineReportingTool.Public.Models.Order;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport.Create;

public partial class TemporaryAddress(
    ILogger<TemporaryAddress> logger,
    ISearchRepository searchRepository,
    ProtectedSessionStorage protectedSessionStorage,
    NavigationManager navigationManager
) : IPageOrder, IAsyncDisposable
{
    // Page order properties
    public string Title { get; set; } = FloodReportCreatePages.TemporaryAddress.Title;
    public IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [
        GeneralPages.Home.ToGdsBreadcrumb(),
        FloodReportPages.Home.ToGdsBreadcrumb(),
        FloodReportCreatePages.Home.ToGdsBreadcrumb(),
        FloodReportCreatePages.Postcode.ToGdsBreadcrumb(),
    ];

    [SupplyParameterFromQuery]
    private bool FromSummary { get; set; }

    [PersistentState]
    public Models.FloodReport.Create.Address Model { get; set; } = null!;

    private IList<GdsOptionItem<long>> _addressOptions = [];
    private EditContext _editContext = default!;
    private readonly CancellationTokenSource _cts = new();
    private bool _isSearching = true;
    private IList<ApiAddress> _addresses = [];

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
            var createExtraData = await GetCreateExtraData();

            Model.Postcode = createExtraData.TemporaryPostcode;
            Model.UPRN = eligibilityCheck.TemporaryUprn == 0 ? null : eligibilityCheck.TemporaryUprn;
            Model.IsAddress = true;
            Model.LocationDesc = eligibilityCheck.TemporaryLocationDesc;
            _addressOptions = await CreateAddressOptions();

            StateHasChanged();

            
        }
    }

    private async Task OnValidSubmit()
    {
        // Remember the selected address
        var apiAddress = _addresses.FirstOrDefault(o => o.UPRN == Model.UPRN);
        if (apiAddress != null)
        {
            var eligibilityCheck = await GetEligibilityCheck();
            var createExtraData = await GetCreateExtraData();

            var updatedEligibilityCheck = eligibilityCheck with
            {
                TemporaryUprn = apiAddress.UPRN,
                TemporaryLocationDesc = apiAddress.ConcatenatedAddress,
            };

            var updatedExtraData = createExtraData with
            {
                TemporaryPostcode = apiAddress.Postcode.ToUpperInvariant(),
            };

            await protectedSessionStorage.SetAsync(SessionConstants.EligibilityCheck, updatedEligibilityCheck);
            await protectedSessionStorage.SetAsync(SessionConstants.EligibilityCheck_ExtraData, updatedExtraData);

            // Go to the next page or pass back to the summary (user must return from property type page)
            var nextPage = FloodReportCreatePages.Vulnerability;
            var nextPageUrl = nextPage.Url;
            if (FromSummary)
            {
                nextPageUrl += "?fromsummary=true";
            }
            navigationManager.NavigateTo(nextPageUrl);
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

        logger.LogWarning("Eligibility Check was not found in the protected storage.");
        return new EligibilityCheckDto();
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

        logger.LogDebug("Eligibility Check > Extra Data was not found in the protected storage.");
        return new();
    }

    private async Task<IList<GdsOptionItem<long>>> CreateAddressOptions()
    {
        _addresses = await AddressSearch();
        return [.. _addresses.Select(CreateOption)];
    }

    /// <summary>
    /// Search for addresses based on the postcode.
    /// </summary>
    private async Task<IList<ApiAddress>> AddressSearch()
    {
        if (string.IsNullOrWhiteSpace(Model.Postcode) && Model.IsAddress == false)
        {
            logger.LogDebug("Non address query so not searching");
            _isSearching = false;
            return [];
        }
        else if (string.IsNullOrWhiteSpace(Model.Postcode))
        {
            logger.LogDebug("No postcode, not searching");
            _isSearching = false;
            return [];
        }

        try
        {
            logger.LogDebug("Searching for addresses with postcode {Postcode}", Model.Postcode);

            _isSearching = true;
            var referer = navigationManager.ToAbsoluteUri("");
            return await searchRepository.AddressSearch(Model.Postcode, SearchAreaOptions.uk, referer, _cts.Token);
        }
        catch (ConfigurationMissingException ex)
        {
            logger.LogError(ex, "There was a configuration problem getting the address information");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "There was an unexpected problem getting the address information");
        }
        finally
        {
            _isSearching = false;
        }

        return [];
    }

    private async Task SearchAgain()
    {
        Model.UPRN = null;
        _addressOptions = await CreateAddressOptions();
    }

    private GdsOptionItem<long> CreateOption(ApiAddress apiAddress)
    {
        var label = apiAddress.ConcatenatedAddress.AsSpan();
        var value = apiAddress.UPRN;
        var selected = value == Model.UPRN;

        return new GdsOptionItem<long>(id: "", label, value, selected);
    }
}

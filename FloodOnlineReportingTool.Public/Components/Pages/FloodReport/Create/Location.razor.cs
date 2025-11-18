using FloodOnlineReportingTool.Database.Exceptions;
using FloodOnlineReportingTool.Database.Models.API;
using FloodOnlineReportingTool.Database.Models.Eligibility;
using FloodOnlineReportingTool.Database.Options;
using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.Public.Models;
using FloodOnlineReportingTool.Public.Models.FloodReport.Create;
using FloodOnlineReportingTool.Public.Models.Order;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using System.Net;
using System.Text.Json;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport.Create;

public partial class Location(
    ILogger<Location> logger,
    ISearchRepository repository,
    ProtectedSessionStorage protectedSessionStorage,
    NavigationManager navigationManager,
    IGdsJsInterop gdsJs,
    IJSRuntime JS,
    IOptions<GISOptions> gisOptions
) : IPageOrder, IAsyncDisposable
{
    // Page order properties
    public string Title { get; set; } = FloodReportCreatePages.Location.Title;
    public IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [
        GeneralPages.Home.ToGdsBreadcrumb(),
        FloodReportPages.Home.ToGdsBreadcrumb(),
        FloodReportCreatePages.Home.ToGdsBreadcrumb(),
    ];

    [SupplyParameterFromQuery]
    private bool FromSummary { get; set; }

    private Models.FloodReport.Create.Location Model { get; set; } = default!;

    private EditContext _editContext = default!;
    private ValidationMessageStore _messageStore = default!;
    private readonly CancellationTokenSource _cts = new();
    private bool _isLoading = true;
    private IJSObjectReference? _module;
    private ElementReference? _map;
    private DotNetObjectReference<Location>? _dotNetReference;

    private readonly GISOptions _gisSettings = gisOptions.Value;

    protected override void OnInitialized()
    {
        // Setup model and edit context
        Model ??= new();
        _editContext = new(Model);
        _messageStore = new(_editContext);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Set any previously entered data
            var eligibilityCheck = await GetEligibilityCheck();
            var createExtraData = await GetCreateExtraData();

            Model.Easting = eligibilityCheck.Easting == 0 ? null : eligibilityCheck.Easting;
            Model.Northing = eligibilityCheck.Northing == 0 ? null : eligibilityCheck.Northing;
            Model.Postcode = createExtraData.Postcode;
            Model.IsAddress = eligibilityCheck.IsAddress;
            Model.LocationDesc = eligibilityCheck.LocationDesc;

            await LoadJavaScriptAndMap();

            _isLoading = false;
            StateHasChanged();
        }
    }

    private async Task LoadJavaScriptAndMap()
    {
        var errorField = FieldIdentifier.Create(() => Model.Easting);

        try
        {
            _dotNetReference = DotNetObjectReference.Create(this);
            await gdsJs.InitGds(_cts.Token);
            _module = await JS.InvokeAsync<IJSObjectReference>("import", _cts.Token, "/js/components/location.js");
            if (_module == null)
            {
                logger.LogError("Failed to load the map JavaScript module.");
                _messageStore.Add(errorField, "There was a problem loading the map. Please try again but if this issue happens again then please report a bug.");
                _editContext.NotifyValidationStateChanged();
                return;
            }

            // Pass the OS key to JavaScript
            var apiKey = _gisSettings.OSApiKey;
            await _module.InvokeVoidAsync("receiveApiKey", _cts.Token, apiKey);

            //Setup the map
            var (centreEasting, centreNorthing) = MapCentre();
            var (startingEasting, startingNorthing) = StartingLocation();
            await _module.InvokeVoidAsync("setupMap", _cts.Token, _map, centreEasting, centreNorthing, startingEasting, startingNorthing);
            await _module.InvokeVoidAsync("setHelper", _cts.Token, _dotNetReference);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "There was an error loading the map and JavaScript.");
            _messageStore.Add(errorField, $"There was a problem loading the map. Please try again but if this issue happens again then please report a bug.");
            _editContext.NotifyValidationStateChanged();
        }
    }

    /// <summary>
    /// Get the centre of the map
    /// </summary>
    /// <remarks>
    /// Defaults to Dorchester if no coordinates are set. This could be adjusted in the future with additional logic.
    /// </remarks>
    private (double, double) MapCentre()
    {
        if (Model.Easting == null || Model.Northing == null)
        {
            return (369082, 90191);
        }

        return (Model.Easting.Value, Model.Northing.Value);
    }

    /// <summary>
    /// Get the starting location of the map
    /// </summary>
    private (double?, double?) StartingLocation()
    {
        if (Model.Easting == null || Model.Northing == null)
        {
            return (null, null);
        }

        return (Model.Easting.Value, Model.Northing.Value);
    }

    private async Task ChooseLocation()
    {
        if (_module != null)
        {
            await _module.InvokeVoidAsync("turnMarkerOn", _cts.Token);
        }
    }

    [JSInvokable("CoordinatesChanged")]
    public Task JS_CoordinatesChanged(float easting, float northing)
    {
        Model.Easting = easting;
        Model.Northing = northing;
        StateHasChanged();

        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        // Dispose the JavaScript map and dispose of the module
        try
        {
            if (_module != null)
            {
                await _module.InvokeVoidAsync("destroyMap", _cts.Token);
                await _module.DisposeAsync();
            }
        }
        catch (Exception)
        {
        }

        // Dispose of everything else
        try
        {
            await _cts.CancelAsync();
            _cts.Dispose();

            _dotNetReference?.Dispose();
        }
        catch (Exception)
        {
        }

        GC.SuppressFinalize(this);
    }

    private async Task OnSubmit()
    {
        _messageStore.Clear();

        if (!_editContext.Validate())
        {
            return;
        }

        await OnValidSubmit();
    }

    private async Task OnValidSubmit()
    {
        var createExtraData = await GetCreateExtraData();
        var eligibilityCheck = await GetEligibilityCheck();
        ExtraData? updatedExtraData = null;
        bool propertyTypeReset = false;
        if (Model.IsAddress)
        {
            Model.Postcode = await GetPostcodeFromLocation();

            updatedExtraData = createExtraData with
            {
                Postcode = Model.Postcode,
                //PrimaryClassification = apiAddress.PrimaryClassification,
                //SecondaryClassification = apiAddress.SecondaryClassification,
            };
            propertyTypeReset = true;

        }
        else if ((Model.Easting != eligibilityCheck.Easting || Model.Northing != eligibilityCheck.Northing))
        {
            //They changed the location so we reset the property type option
            updatedExtraData = createExtraData with
            {
                Postcode = null,
                PrimaryClassification = null,
                SecondaryClassification = null,
                PropertyType = null,
            };
            propertyTypeReset = true;
        }
        if (updatedExtraData != null)
        {
            await protectedSessionStorage.SetAsync(SessionConstants.EligibilityCheck_ExtraData, updatedExtraData);
        }

        // Remember the entered postcode
        var updatedEligibilityCheck = eligibilityCheck with
        {
            //Uprn = apiAddress.UPRN,
            Easting = Model.Easting!.Value,
            Northing = Model.Northing!.Value,
            LocationDesc = Model.LocationDesc,

        };
        await protectedSessionStorage.SetAsync(SessionConstants.EligibilityCheck, updatedEligibilityCheck);

        // Go to the next page or pass back to the summary (user must return from property type page if reset)
        var nextPage = GetNextPage(propertyTypeReset);
        var nextPageUrl = nextPage.Url;
        if (propertyTypeReset && FromSummary)
        {
            nextPageUrl += "?fromsummary=true";
        }
        navigationManager.NavigateTo(nextPageUrl);
    }
    private PageInfo GetNextPage(bool propertyTypeReset)
    {
        if (!propertyTypeReset && FromSummary)
        {
            return FloodReportCreatePages.Summary;
        }

        if (Model.IsAddress)
        {
            return FloodReportCreatePages.Address;
        }

        return FloodReportCreatePages.PropertyType;
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

    /// <summary>
    /// Get the postcode from the API
    /// </summary>
    private async Task<string?> GetPostcodeFromLocation()
    {
        // Null check, this is handled by the validator, but returning early just in case
        if (Model.Easting == null || Model.Northing == null)
        {
            return null;
        }
        var easting = Model.Easting.Value;
        var northing = Model.Northing.Value;

        try
        {
            var referrer = navigationManager.ToAbsoluteUri("");
            var response = await repository.GetNearestAddressResponse(easting, northing, SearchAreaOptions.uk, referrer, _cts.Token);

            if (response == null || response.StatusCode == HttpStatusCode.NotFound)
            {
                logger.LogWarning("No postcode found for easting {Easting} and northing {Northing}", easting, northing);
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Nearest postcode response status code {StatusCode}", response.StatusCode);
                return null;
            }

            var addresses = await response.Content.ReadFromJsonAsync<List<ApiAddress>>(_cts.Token);
            if (addresses == null || addresses?.Count == 0)
            {
                logger.LogWarning("No postcode found for easting {Easting} and northing {Northing}", easting, northing);
                return null;
            }

            var postcode = addresses?.FirstOrDefault()?.Postcode ?? "";
            return postcode.Trim().ToUpperInvariant();
        }
        catch (ConfigurationMissingException ex)
        {
            logger.LogError(ex, "There was a configuration problem getting the nearest postcode");
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "There was a problem converting the nearest postcode response");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "There was an unexpected problem getting the nearest postcode");
        }

        return null;
    }

}

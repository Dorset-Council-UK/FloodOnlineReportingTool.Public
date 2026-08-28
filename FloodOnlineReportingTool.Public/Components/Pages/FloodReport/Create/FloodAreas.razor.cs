using FloodOnlineReportingTool.Database.Models.Eligibility;
using FloodOnlineReportingTool.Database.Models.Flood;
using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.Public.Models;
using FloodOnlineReportingTool.Public.Models.FloodReport.Create;
using FloodOnlineReportingTool.Public.Models.Order;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport.Create;

public partial class FloodAreas(
    ILogger<FloodAreas> logger,
    ICommonRepository commonRepository,
    ProtectedSessionStorage protectedSessionStorage,
    NavigationManager navigationManager
) : IAsyncDisposable
{
    // Page order properties
    public string Title { get; set; } = FloodReportCreatePages.FloodAreas.Title;

    [SupplyParameterFromQuery]
    private bool FromSummary { get; set; }
    private PageInfo PreviousPage => FromSummary
        ? FloodReportCreatePages.Summary
        : FloodReportCreatePages.PropertyType;

    private Models.FloodReport.Create.FloodAreas Model { get; set; } = default!;

    private EditContext _editContext = default!;
    private readonly CancellationTokenSource _cts = new();
    private bool _isLoading = true;
    private IList<FloodImpact> ResidentialOptions { get; set; } = [];
    private IList<FloodImpact> CommercialOptions { get; set; } = [];
    private Dictionary<string, bool> SelectedResidentialOptions = [];
    private Dictionary<string, bool> SelectedCommercialOptions = [];

    protected override async Task OnInitializedAsync()
    {
        if (Model is null)
        {
            // Setup model and edit context
            Model ??= new();
            _editContext = new(Model);
            _editContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
        }

        ResidentialOptions = await commonRepository.GetFloodImpactsByCategory(FloodImpactCategory.Residential, _cts.Token);
        CommercialOptions = await commonRepository.GetFloodImpactsByCategory(FloodImpactCategory.Commercial, _cts.Token);
        UpdateSelectedResidentialOptions();
        UpdateSelectedCommercialOptions();
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

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Set any previously entered data
            var eligibilityCheck = await GetEligibilityCheck();
            var createExtraData = await GetCreateExtraData();

            var _propertyTypeId = await GetPropertyTypeId(createExtraData);
            if (_propertyTypeId != null)
            {
                // The property types are Residential, Commercial, Other, Not Specified
                Model.ShowResidential = _propertyTypeId != FloodImpactIds.Commercial; // Allowed types are Residential, Other, Not Specified
                Model.ShowCommercial = _propertyTypeId != FloodImpactIds.Residential; // Allowed types are Commercial, Other, Not Specified
            }

            Model.IsUninhabitable = eligibilityCheck.Uninhabitable;
            if (Model.ShowResidential)
            {
                // update model and the selected options
                Model.Residentials = eligibilityCheck.Residentials;
                UpdateSelectedResidentialOptions();
            }
            if (Model.ShowCommercial)
            {
                // update model and the selected options
                Model.Commercials = eligibilityCheck.Commercials;
                UpdateSelectedCommercialOptions();
            }

            _isLoading = false;
            StateHasChanged(); 
        }
    }

    private async Task<Guid?> GetPropertyTypeId(ExtraData extraData)
    {
        if (extraData.PropertyType == null)
        {
            return null;
        }

        var floodImpact = await commonRepository.GetFloodImpact(extraData.PropertyType.Value, _cts.Token);
        if (floodImpact == null)
        {
            return null;
        }

        return floodImpact.Id;
    }

    private async Task OnSubmit()
    {
        if (_editContext.Validate())
        {
            await OnValidSubmit();
        }
    }

    private async Task OnValidSubmit()
    {
        // Update the eligibility check
        var eligibilityCheck = await GetEligibilityCheck();
        var createExtraData = await GetCreateExtraData();

        // We need to clear any temporary address data that might be stored if they click No
        bool runTemporaryAddress = Model.IsUninhabitable is null ? false : (bool)Model.IsUninhabitable;
        var updated = runTemporaryAddress ? eligibilityCheck with
        {
            Uninhabitable = Model.IsUninhabitable,
            Residentials = Model.Residentials,
            Commercials = Model.Commercials,
        } : eligibilityCheck with
        {
            Uninhabitable = Model.IsUninhabitable,
            Residentials = Model.Residentials,
            Commercials = Model.Commercials,
            TemporaryLocationDesc = null,
            TemporaryUprn = null,
        };
        if (runTemporaryAddress == false)
        {
            var updatedExtraData = createExtraData with
            {
                TemporaryPostcode = null,
            };
            await protectedSessionStorage.SetAsync(SessionConstants.EligibilityCheck_ExtraData, updatedExtraData);
        }
        await protectedSessionStorage.SetAsync(SessionConstants.EligibilityCheck, updated);

        // Go to the next page or back to the summary
        var nextPage = FromSummary
            ? FloodReportCreatePages.Summary
            : runTemporaryAddress ? FloodReportCreatePages.TemporaryPostcode : FloodReportCreatePages.Vulnerability;
        navigationManager.NavigateTo(nextPage.Url);
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
        return new();
    }

    /// <summary>
    /// Set up the selected residential options (string, bool dictionary)
    /// </summary>
    private void UpdateSelectedResidentialOptions()
    {
        SelectedResidentialOptions = ResidentialOptions.ToDictionary(o => o.Id.ToString("N"), o => Model.Residentials.Contains(o.Id), StringComparer.Ordinal);
    }

    /// <summary>
    /// Set up the selected commercial options (string, bool dictionary)
    /// </summary>
    private void UpdateSelectedCommercialOptions()
    {
        SelectedCommercialOptions = CommercialOptions.ToDictionary(o => o.Id.ToString("N"), o => Model.Commercials.Contains(o.Id), StringComparer.Ordinal);
    }

    private void OnResidentialChanged(bool isChecked, Guid floodSourceId)
    {
        // update the model
        if (isChecked && !Model.Residentials.Contains(floodSourceId))
            Model.Residentials.Add(floodSourceId);
        else if (!isChecked)
            Model.Residentials.Remove(floodSourceId);
    }

    private void OnCommercialChanged(bool isChecked, Guid floodSourceId)
    {
        // update the model
        if (isChecked && !Model.Commercials.Contains(floodSourceId))
            Model.Commercials.Add(floodSourceId);
        else if (!isChecked)
            Model.Commercials.Remove(floodSourceId);
    }
}

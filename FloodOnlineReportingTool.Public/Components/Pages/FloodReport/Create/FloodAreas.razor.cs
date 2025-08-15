using FloodOnlineReportingTool.Database.Models;
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
    NavigationManager navigationManager,
    IGdsJsInterop gdsJs
) : IPageOrder, IAsyncDisposable
{
    // Page order properties
    public string Title { get; set; } = FloodReportCreatePages.FloodAreas.Title;
    public IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [
        GeneralPages.Home.ToGdsBreadcrumb(),
        FloodReportPages.Home.ToGdsBreadcrumb(),
        FloodReportCreatePages.PropertyType.ToGdsBreadcrumb(),
    ];

    [SupplyParameterFromQuery]
    private bool FromSummary { get; set; }

    private Models.FloodReport.Create.FloodAreas Model { get; set; } = default!;

    private EditContext _editContext = default!;
    private readonly CancellationTokenSource _cts = new();
    private bool _isLoading = true;
    private readonly IReadOnlyCollection<GdsOptionItem<bool>> _uninhabitableOptions = [
        new("uninhabitable-yes", "Yes", value: true),
        new("uninhabitable-no", "No", value: false),
    ];

    protected override void OnInitialized()
    {
        // Setup model and edit context
        Model ??= new();
        _editContext = new(Model);
        _editContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
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
                var options = await CreateResidentialOptions(eligibilityCheck.Residentials);
                Model.ResidentialOptions = [.. options];
            }
            if (Model.ShowCommercial)
            {
                var options = await CreateCommercialOptions(eligibilityCheck.Commercials);
                Model.CommercialOptions = [.. options];
            }

            _isLoading = false;
            StateHasChanged();

            await gdsJs.InitGds(_cts.Token);
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

    private async Task OnValidSubmit()
    {
        // Update the eligibility check
        var eligibilityCheck = await GetEligibilityCheck();
        var updated = eligibilityCheck with
        {
            Uninhabitable = Model.IsUninhabitable,
            Residentials = [.. Model.ResidentialOptions.Where(o => o.Selected).Select(o => o.Value)],
            Commercials = [.. Model.CommercialOptions.Where(o => o.Selected).Select(o => o.Value)],
        };

        await protectedSessionStorage.SetAsync(SessionConstants.EligibilityCheck, updated);

        // Go to the next page or back to the summary
        var nextPage = FromSummary ? FloodReportCreatePages.Summary : FloodReportCreatePages.Vulnerability;
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

    private async Task<IList<GdsOptionItem<Guid>>> CreateResidentialOptions(IList<Guid> selectedIds)
    {
        var floodImpacts = await commonRepository.GetFloodImpactsByCategory(FloodImpactCategory.Residential, _cts.Token);
        return [.. floodImpacts.Select((o, idx) => CreateOption(o, $"residential-impact-item-{idx}", selectedIds))];
    }

    private async Task<IList<GdsOptionItem<Guid>>> CreateCommercialOptions(IList<Guid> selectedIds)
    {
        var floodImpacts = await commonRepository.GetFloodImpactsByCategory(FloodImpactCategory.Commercial, _cts.Token);
        return [.. floodImpacts.Select((o, idx) => CreateOption(o, $"commercial-impact-item-{idx}", selectedIds))];
    }

    private static GdsOptionItem<Guid> CreateOption(FloodImpact floodImpact, ReadOnlySpan<char> id, IList<Guid> selectedIds)
    {
        var label = floodImpact.TypeName == null ? [] : floodImpact.TypeName.AsSpan();
        var isExclusive = floodImpact.Id == FloodImpactIds.ZoneRNotSure || floodImpact.Id == FloodImpactIds.ZoneCNotSure;
        var selected = selectedIds.Contains(floodImpact.Id);

        return new GdsOptionItem<Guid>(id, label, floodImpact.Id, selected, isExclusive);
    }
}

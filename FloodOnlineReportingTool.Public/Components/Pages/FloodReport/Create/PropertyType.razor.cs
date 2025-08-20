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

public partial class PropertyType(
    ILogger<PropertyType> logger,
    ICommonRepository commonRepository,
    ProtectedSessionStorage protectedSessionStorage,
    NavigationManager navigationManager,
    IGdsJsInterop gdsJs
) : IPageOrder, IAsyncDisposable
{
    // Page order properties
    public string Title { get; set; } = FloodReportCreatePages.PropertyType.Title;

    [SupplyParameterFromQuery]
    private bool FromLocation { get; set; }

    public IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [
        GeneralPages.Home.ToGdsBreadcrumb(),
        FloodReportPages.Home.ToGdsBreadcrumb(),
        FloodReportCreatePages.Home.ToGdsBreadcrumb(),
    ];

    [SupplyParameterFromQuery]
    private bool FromSummary { get; set; }

    private Models.FloodReport.Create.PropertyType Model { get; set; } = default!;

    private EditContext _editContext = default!;
    private readonly CancellationTokenSource _cts = new();
    private bool _isLoading = true;
    private string? _addressPreview;
    private string? _classification;

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
            Model.FromLocation = FromLocation;

            // Set any previously entered data
            var eligibilityCheck = await GetEligibilityCheck();
            var createExtraData = await GetCreateExtraData();

            _addressPreview = eligibilityCheck.LocationDesc;
            _classification = Classification(createExtraData.PrimaryClassification, createExtraData.SecondaryClassification);

            var propertyTypes = await commonRepository.GetFloodImpactsByCategory(FloodImpactCategory.PropertyType, _cts.Token);
            Model.Property = GetPropertyType(createExtraData, propertyTypes);
            Model.PropertyOptions = [.. propertyTypes.Select(CreateOption)];

            var organisations = await commonRepository.GetResponsibleOrganisations(eligibilityCheck.Easting, eligibilityCheck.Northing, _cts.Token);
            Model.ResponsibleOrganisations = organisations.AsReadOnly();

            _isLoading = false;
            StateHasChanged();

            await gdsJs.InitGds(_cts.Token);
        }
    }

    private async Task OnValidSubmit()
    {
        // Save the selected property type
        var createExtraData = await GetCreateExtraData();
        var updatedExtraData = createExtraData with
        {
            PropertyType = Model.Property,
        };
        await protectedSessionStorage.SetAsync(SessionConstants.EligibilityCheck_ExtraData, updatedExtraData);

        // Go to the next page or back to the summary
        var nextPage = FromSummary ? FloodReportCreatePages.Summary : FloodReportCreatePages.FloodAreas;
        navigationManager.NavigateTo(nextPage.Url);
    }

    private static string Classification(ReadOnlySpan<char> primaryClassification, ReadOnlySpan<char> secondaryClassification)
    {
        var hasPrimary = !primaryClassification.IsEmpty && !primaryClassification.IsWhiteSpace();
        var hasSecondary = !secondaryClassification.IsEmpty && !secondaryClassification.IsWhiteSpace();

        if (hasPrimary && hasSecondary)
        {
            return string.Concat(primaryClassification, " and ", secondaryClassification);
        }

        if (hasPrimary)
        {
            return primaryClassification.ToString();
        }

        if (hasSecondary)
        {
            return secondaryClassification.ToString();
        }

        return string.Empty;
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
    /// Work out the property type. The priority is as follows: previously stored value, primary classification, 'other'.
    /// </summary>
    private static Guid? GetPropertyType(ExtraData createExtraData, IList<FloodImpact> floodImpacts)
    {
        if (createExtraData.PropertyType != null)
        {
            return createExtraData.PropertyType;
        }

        // Use the primary classification to find the flood impact
        if (createExtraData.PrimaryClassification != null)
        {
            Guid? propertyType = floodImpacts
                .Where(o => o.TypeName != null && o.TypeName.Equals(createExtraData.PrimaryClassification, StringComparison.CurrentCultureIgnoreCase))
                .Select(o => o.Id)
                .FirstOrDefault();
            if (propertyType != null)
            {
                return propertyType.Value;
            }

            return FloodImpactIds.PropertyTypeOther;
        }

        return null;
    }

    private GdsOptionItem<Guid> CreateOption(FloodImpact floodImpact)
    {
        var label = floodImpact.TypeName == null ? [] : floodImpact.TypeName.AsSpan();
        var id = floodImpact.TypeName == null ? "property-unknown" : $"property-{floodImpact.TypeName.Replace(' ', '-').ToLowerInvariant()}";
        var selected = floodImpact.Id == Model.Property;

        return new GdsOptionItem<Guid>(id, label, value: floodImpact.Id, selected);
    }
}

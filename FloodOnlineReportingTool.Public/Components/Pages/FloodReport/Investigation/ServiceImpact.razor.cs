using FloodOnlineReportingTool.Database.Models.Flood;
using FloodOnlineReportingTool.Database.Models.Investigate;
using FloodOnlineReportingTool.Database.Models.Status;
using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.Public.Models;
using FloodOnlineReportingTool.Public.Models.Order;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport.Investigation;

[Authorize]
public partial class ServiceImpact(
    ILogger<ServiceImpact> logger,
    ICommonRepository commonRepository,
    ProtectedSessionStorage protectedSessionStorage,
    NavigationManager navigationManager
) : IAsyncDisposable
{
    [SupplyParameterFromQuery]
    private bool FromSummary { get; set; }
    private PageInfo NextPage => FromSummary
        ? InvestigationPages.Summary
        : InvestigationPages.CommunityImpact;
    private static PageInfo PreviousPage => InvestigationPages.PeakDepth;

    private Models.FloodReport.Investigation.ServiceImpact Model { get; set; } = default!;
    private IReadOnlyCollection<GdsOptionItem<Guid>> _wereServicesImpactedOptions = [];

    private EditContext _editContext = default!;
    private readonly CancellationTokenSource _cts = new();
    private bool _isLoading = true;

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
            var investigation = await GetInvestigation();

            // Get all the service impacts (flood impacts) This includes not sure, and services not affected
            var floodImpacts = await commonRepository.GetFloodImpactsByCategory(FloodImpactCategory.ServiceImpact, _cts.Token);

            // Build the yes, no, not sure radio buttons
            Guid? selectedValue = investigation.ServiceImpacts switch
            {
                var impacts when impacts.Contains(FloodImpactIds.ServiceImpactNotSure) => FloodImpactIds.ServiceImpactNotSure,
                var impacts when impacts.Contains(FloodImpactIds.ServicesNotAffected) => FloodImpactIds.ServicesNotAffected,
                var impacts when impacts.Count > 0 => RecordStatusIds.Yes,
                _ => null,
            };
            Model.WereServicesImpactedId = selectedValue;
            _wereServicesImpactedOptions = await CreateWereServicesImpactedOptions(floodImpacts, selectedValue);

            // Build the yes > services impacted checkboxes
            Model.ImpactedServicesOptions = await CreateImpactedServicesOptions(floodImpacts, investigation.ServiceImpacts);

            _isLoading = false;
            StateHasChanged();
        }
    }

    private async Task OnValidSubmit()
    {
        // update the investigation with the new service impacts
        var investigation = await GetInvestigation();
        var updatedInvestigation = investigation with
        {
            ServiceImpacts = GetSelectedServiceImpacts(),
        };
        await protectedSessionStorage.SetAsync(SessionConstants.Investigation, updatedInvestigation);

        // Go to the next page or back to the summary
        navigationManager.NavigateTo(NextPage.Url);
    }

    private IList<Guid> GetSelectedServiceImpacts()
    {
        // no or not sure selected, return the service impact id
        if (Model.WereServicesImpactedId.Equals(FloodImpactIds.ServicesNotAffected) || Model.WereServicesImpactedId.Equals(FloodImpactIds.ServiceImpactNotSure))
        {
            return [Model.WereServicesImpactedId.Value];
        }

        // yes selected, return all the selected service impact ids
        return [.. Model.ImpactedServicesOptions.Where(o => o.Selected).Select(o => o.Value)];
    }

    private async Task<InvestigationDto> GetInvestigation()
    {
        var data = await protectedSessionStorage.GetAsync<InvestigationDto>(SessionConstants.Investigation);
        if (data is { Success: true, Value: not null })
        {
            return data.Value;
        }

        logger.LogWarning("Investigation was not found in the protected storage.");
        return new InvestigationDto();
    }


    private async Task<IReadOnlyCollection<GdsOptionItem<Guid>>> CreateWereServicesImpactedOptions(IList<FloodImpact> floodImpacts, Guid? selectedValue)
    {
        const string idPrefix = "were-services-impacted";

        var yesRecordStatus = await commonRepository.GetRecordStatus(RecordStatusIds.Yes, _cts.Token);
        var noFloodImpact = floodImpacts.FirstOrDefault(fi => fi.Id == FloodImpactIds.ServicesNotAffected);
        var notSureFloodImpact = floodImpacts.FirstOrDefault(fi => fi.Id == FloodImpactIds.ServiceImpactNotSure);

        if (yesRecordStatus is null || noFloodImpact is null || notSureFloodImpact is null)
        {
            return [];
        }

        // Change the label for services not affected
        noFloodImpact = noFloodImpact with
        {
            TypeName = "No",
        };

        return [
            CreateOption(yesRecordStatus, idPrefix, selectedValue),
            CreateOption(noFloodImpact, idPrefix, selectedValue),
            CreateOption(notSureFloodImpact, idPrefix, selectedValue),
        ];
    }

    private static async Task<IReadOnlyCollection<GdsOptionItem<Guid>>> CreateImpactedServicesOptions(IList<FloodImpact> floodImpacts, IList<Guid> selectedValues)
    {
        const string idPrefix = "impacted-services";
        var withoutNotSureAndNotAffected = floodImpacts.Where(fi => fi.Id != FloodImpactIds.ServicesNotAffected && fi.Id != FloodImpactIds.ServiceImpactNotSure);
        return [.. withoutNotSureAndNotAffected.Select(o => CreateOption(o, idPrefix, selectedValues))];
    }

    /// <summary>
    /// Create a GdsOptionItem from a FloodImpact, with possible multiple selections
    /// </summary>
    private static GdsOptionItem<Guid> CreateOption(FloodImpact floodImpact, string idPrefix, IList<Guid> selectedValues)
    {
        var id = $"{idPrefix}-{floodImpact.Id}".AsSpan();
        var label = floodImpact.TypeName.AsSpan();
        var selected = selectedValues.Contains(floodImpact.Id);
        var isExclusive = floodImpact.Id == FloodImpactIds.ServicesNotAffected || floodImpact.Id == FloodImpactIds.ServiceImpactNotSure;

        return new GdsOptionItem<Guid>(id, label, floodImpact.Id, selected, isExclusive);
    }

    /// <summary>
    /// Create a GdsOptionItem from a FloodImpact, with an optional single selection
    /// </summary>
    private static GdsOptionItem<Guid> CreateOption(FloodImpact floodImpact, string idPrefix, Guid? selectedValue)
    {
        var id = $"{idPrefix}-{floodImpact.Id}".AsSpan();
        var label = floodImpact.TypeName.AsSpan();
        var selected = selectedValue?.Equals(floodImpact.Id) ?? false;
        var isExclusive = floodImpact.Id.Equals(FloodImpactIds.ServiceImpactNotSure);

        return new GdsOptionItem<Guid>(id, label, floodImpact.Id, selected, isExclusive);
    }

    /// <summary>
    /// Create a GdsOptionItem from a RecordStatus, with an optional single selection
    /// </summary>
    private static GdsOptionItem<Guid> CreateOption(RecordStatus recordStatus, string idPrefix, Guid? selectedValue)
    {
        var id = $"{idPrefix}-{recordStatus.Id}".AsSpan();
        var label = recordStatus.Text.AsSpan();
        var selected = selectedValue?.Equals(recordStatus.Id) ?? false;

        return new GdsOptionItem<Guid>(id, label, recordStatus.Id, selected);
    }
}
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
   
    private EditContext _editContext = default!;
    private readonly CancellationTokenSource _cts = new();
    private bool _isLoading = true;
    private IList<RecordStatus> ServiceImpactRadioOptions = [];
    private IList<FloodImpact> ServiceImpactCheckboxOptions = [];
    private Dictionary<string, bool> SelectedServiceImpactCheckboxOptions = [];

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

    protected override async Task OnInitializedAsync()
    {
        if (Model is null)
        {
            // Setup model and edit context
            Model ??= new();
            _editContext = new(Model);
            _editContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
        }

        ServiceImpactRadioOptions = await commonRepository.GetRecordStatusesByCategory(RecordStatusCategory.General, _cts.Token);
        ServiceImpactCheckboxOptions = await commonRepository.GetFloodImpactsByCategory(FloodImpactCategory.ServiceImpact, _cts.Token);
        UpdateSelectedServiceImpactOptions();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Set any previously entered data
            var investigation = await GetInvestigation();
            Model.WereServicesImpactedId = investigation.ServiceImpacts switch
            {
                var impacts when impacts.Contains(FloodImpactIds.ServiceImpactNotSure) => FloodImpactIds.ServiceImpactNotSure,
                var impacts when impacts.Contains(FloodImpactIds.ServicesNotAffected) => FloodImpactIds.ServicesNotAffected,
                var impacts when impacts.Count > 0 => RecordStatusIds.Yes,
                _ => null,
            };
            Model.ImpactedServicesOptions = [.. investigation.ServiceImpacts];
            UpdateSelectedServiceImpactOptions();

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
            IsPeakDepthKnownId = investigation.IsPeakDepthKnownId,
        };
        await protectedSessionStorage.SetAsync(SessionConstants.Investigation, updatedInvestigation);

        // Go to the next page or back to the summary
        navigationManager.NavigateTo(NextPage.Url);
    }

    private IList<Guid> GetSelectedServiceImpacts()
    {
        // no or not sure selected, return the service impact id
        if (Model.WereServicesImpactedId == FloodImpactIds.ServicesNotAffected || Model.WereServicesImpactedId == FloodImpactIds.ServiceImpactNotSure)
        {
            return [Model.WereServicesImpactedId.Value];
        }

        // yes selected, return all the selected service impact ids
        return [.. Model.ImpactedServicesOptions];
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

    /// <summary>
    /// Set up the selected service impact options (string, bool dictionary)
    /// </summary>
    private void UpdateSelectedServiceImpactOptions()
    {
        SelectedServiceImpactCheckboxOptions = ServiceImpactCheckboxOptions.ToDictionary(o => o.Id.ToString("N"), o => Model.ImpactedServicesOptions.Contains(o.Id), StringComparer.Ordinal);
    }

    private void OnServiceImpactChanged(bool isChecked, Guid floodImpactId)
    {
        // update the model
        if (isChecked && !Model.ImpactedServicesOptions.Contains(floodImpactId))
            Model.ImpactedServicesOptions.Add(floodImpactId);
        else if (!isChecked)
            Model.ImpactedServicesOptions.Remove(floodImpactId);
    }

}
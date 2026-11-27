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
    private ValidationMessageStore _messageStore = default!;
    private readonly CancellationTokenSource _cts = new();
    private bool _isLoading = true;
    private RecordStatus? _yesOption;
    private FloodImpact? _noOption;
    private FloodImpact? _notSureOption;
    private IList<FloodImpact> _serviceImpacts = [];
    private Dictionary<string, bool> SelectedServiceImpact = [];

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
        if (Model is null)
        {
            // Setup model and edit context
            Model ??= new();
            _editContext = new(Model);
            _editContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
            _messageStore = new(_editContext);
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // create the yes, no, not sure questions
            _yesOption = await commonRepository.GetRecordStatus(RecordStatusIds.Yes, _cts.Token);
            _noOption = await commonRepository.GetFloodImpact(FloodImpactIds.ServicesNotAffected, _cts.Token);
            _notSureOption = await commonRepository.GetFloodImpact(FloodImpactIds.ServiceImpactNotSure, _cts.Token);

            // get the service impacts
            var floodImpacts = await commonRepository.GetFloodImpactsByCategory(FloodImpactCategory.ServiceImpact, _cts.Token);
            _serviceImpacts = [.. floodImpacts.Where(o => o.Id != FloodImpactIds.ServicesNotAffected && o.Id != FloodImpactIds.ServiceImpactNotSure)];

            // Get any previously entered data
            var investigation = await GetInvestigation();

            // Update the model with the saved data from the investigation
            if (investigation.ServiceImpacts.Contains(FloodImpactIds.ServiceImpactNotSure))
            {
                Model.WereServicesImpactedId = FloodImpactIds.ServiceImpactNotSure;
            }
            else if (investigation.ServiceImpacts.Contains(FloodImpactIds.ServicesNotAffected))
            {
                Model.WereServicesImpactedId = FloodImpactIds.ServicesNotAffected;
            }
            else if (investigation.ServiceImpacts.Count > 0)
            {
                Model.WereServicesImpactedId = RecordStatusIds.Yes;
                Model.ImpactedServices = [.. investigation.ServiceImpacts];
            }

            // set up the selected service impacts (string, bool dictionary)
            SelectedServiceImpact = _serviceImpacts.ToDictionary(o => o.Id.ToString("N"), o => Model.ImpactedServices.Contains(o.Id), StringComparer.Ordinal);

            _isLoading = false;
            StateHasChanged();

            ValidateOptions();
        }
    }

    private void ValidateOptions()
    {
        if (_yesOption is null || _noOption is null || _notSureOption is null || _serviceImpacts.Count == 0)
        {
            if (_yesOption is null)
            {
                logger.LogError("Failed to load 'Yes' record status option from the database.");
            }

            if (_noOption is null)
            {
                logger.LogError("Failed to load 'No' flood impact option from the database.");
            }

            if (_notSureOption is null)
            {
                logger.LogError("Failed to load 'Not Sure' flood impact option from the database.");
            }

            if (_serviceImpacts.Count == 0)
            {
                logger.LogError("Failed to load service impact options from the database.");
            }

            _messageStore.Add(_editContext.Field(nameof(Model.WereServicesImpactedId)), "Unable to load question options. Please try again but if this issue happens again then please report a bug.");
            _editContext.NotifyValidationStateChanged();
        }
    }

    private async Task OnValidSubmit()
    {
        List<Guid> serviceImpacts = [];
        if (Model.WereServicesImpactedId == RecordStatusIds.Yes)
        {
            // Yes, save all the impacted service IDs
            serviceImpacts = [.. Model.ImpactedServices];
        }
        else if (Model.WereServicesImpactedId.HasValue)
        {
            // No, or Not Sure
            serviceImpacts = [Model.WereServicesImpactedId.Value];
        }

        // update the investigation with the new service impacts
        var investigation = await GetInvestigation();
        var updatedInvestigation = investigation with
        {
            ServiceImpacts = serviceImpacts,
        };
        await protectedSessionStorage.SetAsync(SessionConstants.Investigation, updatedInvestigation);

        // Go to the next page or back to the summary
        navigationManager.NavigateTo(NextPage.Url);
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

    private void OnServiceImpactChanged(bool isChecked, Guid serviceImpactId)
    {
        // update the model using the selected service impact
        if (isChecked)
        {
            if (!Model.ImpactedServices.Contains(serviceImpactId))
            {
                Model.ImpactedServices.Add(serviceImpactId);
            }
        }
        else
        {
            Model.ImpactedServices.Remove(serviceImpactId);
        }
    }

}
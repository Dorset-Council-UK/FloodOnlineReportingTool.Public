using FloodOnlineReportingTool.Contracts.Shared;
using FloodOnlineReportingTool.Database.Models.Eligibility;
using FloodOnlineReportingTool.Database.Models.Flood;
using FloodOnlineReportingTool.Database.Models.Flood.FloodProblemIds;
using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.Public.Models;
using FloodOnlineReportingTool.Public.Models.FloodReport.Create;
using FloodOnlineReportingTool.Public.Models.Order;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport.Create;

public partial class Summary(
    ILogger<Summary> logger,
    ICommonRepository commonRepository,
    IFloodReportRepository floodReportRepository,
    ProtectedSessionStorage protectedSessionStorage,
    NavigationManager navigationManager
) : IAsyncDisposable
{
    private readonly IReadOnlyCollection<GdsBreadcrumb> _breadcrumbs = [
        GeneralPages.Home.ToGdsBreadcrumb(),
        FloodReportPages.Home.ToGdsBreadcrumb(),
        FloodReportCreatePages.FloodSource.ToGdsBreadcrumb(),
    ];

    private EligibilityCheckDto? _eligibilityCheckDto;
    private ExtraData? _createExtraData;
    private readonly ICollection<string> _summaryErrors = [];
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
            // Ignore any exceptions that occur during disposal
        }

        GC.SuppressFinalize(this);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _eligibilityCheckDto = await GetEligibilityCheckDto();
            _createExtraData = await GetCreateExtraData();

            _isLoading = false;
            StateHasChanged();





            // Set any previously entered data
            var createExtraData = await GetCreateExtraData();
            Model.PropertyTypeName = await GetPropertyTypeName(createExtraData);

            var eligibilityCheck = await GetEligibilityCheck();
            Model.IsAddress = eligibilityCheck.IsAddress;
            Model.AddressPreview = eligibilityCheck.LocationDesc;
            Model.TemporaryAddressPreview = eligibilityCheck.TemporaryLocationDesc;
            Model.FloodedAreas = await GetFloodedAreas(eligibilityCheck);
            Model.FloodSources = await GetFloodSources(eligibilityCheck);
            bool runoff = eligibilityCheck.Sources.Any(s => s == PrimaryCauseIds.RainwaterFlowingOverTheGround);
            Model.FloodSecondarySources = runoff ? await GetFloodSecondarySources(eligibilityCheck) : null;
            Model.IsUninhabitable = eligibilityCheck.Uninhabitable;
            Model.StartDate = eligibilityCheck.ImpactStart;
            Model.IsOnGoing = eligibilityCheck.OnGoing;
            Model.FloodDurationKnownId = eligibilityCheck.DurationKnownId;
            Model.VulnerablePeopleId = eligibilityCheck.VulnerablePeopleId;
            Model.NumberOfVulnerablePeople = eligibilityCheck.VulnerableCount;

            // Build the flood lasted for message
            Model.FloodingLasted = null;
            var durationId = eligibilityCheck.DurationKnownId;
            if (!eligibilityCheck.OnGoing && durationId != null)
            {
                if (durationId.Value == FloodDurationIds.DurationKnown && eligibilityCheck.ImpactDuration != null)
                {
                    var duration = TimeSpan.FromHours(eligibilityCheck.ImpactDuration.Value);
                    Model.FloodingLasted = duration.GdsReadable();
                }
                else
                {
                    var floodDuration = await commonRepository.GetFloodProblemByCategory(FloodProblemCategory.Duration, durationId.Value, _cts.Token);
                    Model.FloodingLasted = floodDuration?.TypeDescription;
                }
            }


 
        }
    }

    private void OnValidationStatusChanged(bool isValid)
    {
        if (isValid)
        {
            _summaryErrors.Clear();
            return;
        }

        logger.LogWarning("Flood report summary is not valid.");
        const string generalMessage = "Your flood report information is not complete. Please check the information below and complete any missing information.";
        if (!_summaryErrors.Contains(generalMessage, StringComparer.OrdinalIgnoreCase))
        {
            _summaryErrors.Add(generalMessage);
        }
    }

    private async Task OnAcceptAndSend()
    {
        _summaryErrors.Clear();

        if (_eligibilityCheckDto is null)
        {
            logger.LogError("Flood report information was not found");
            _summaryErrors.Add("Not able to find the flood report information");
            return;
        }

        await SaveFloodReport(_eligibilityCheckDto);
    }

    /// <summary>
    /// Save the flood report, and eligibility check
    /// </summary>
    private async Task SaveFloodReport(EligibilityCheckDto dto)
    {
        const string saveErrorMessage = "Sorry there was a problem saving your flood report. Please try again but if this issue happens again then please report a bug.";

        try
        {
            // Create a new flood report
            var floodReport = await floodReportRepository.CreateWithEligiblityCheck(dto, _cts.Token);

            if (floodReport.EligibilityCheck is null)
            {
                logger.LogError("Failed to create eligibility check for flood report reference {Reference}", floodReport.Reference);
                _summaryErrors.Add(saveErrorMessage);
                return;
            }

            // Clear the stored data
            await protectedSessionStorage.DeleteAsync(SessionConstants.EligibilityCheck);
            await protectedSessionStorage.DeleteAsync(SessionConstants.EligibilityCheck_ExtraData);

            // Navigate to the confirmation page with the reference number
            logger.LogInformation("Flood report created successfully");
            var parameters = new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                { "Reference", floodReport.Reference },
            };
            var confirmationUrl = navigationManager.GetUriWithQueryParameters(FloodReportCreatePages.Confirmation.Url, parameters.AsReadOnly());
            navigationManager.NavigateTo(confirmationUrl);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "There was a problem creating the flood report or saving the eligibility check");
            _summaryErrors.Add(saveErrorMessage);
        }
    }




    private async Task<EligibilityCheckDto> GetEligibilityCheckDto()
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

        logger.LogWarning("Eligibility Check > Extra Data was not found in the protected storage.");
        return new();
    }

    private async Task<string?> GetPropertyTypeName(ExtraData extraData)
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

        return floodImpact.TypeName;
    }

    private async Task<IReadOnlyCollection<string>> GetFloodedAreas(EligibilityCheckDto eligibilityCheck)
    {
        var labels = new List<string>();

        var residentialIds = new HashSet<Guid>(eligibilityCheck.Residentials);
        if (residentialIds.Count > 0)
        {
            var residentials = await commonRepository.GetFloodImpactsByCategory(FloodImpactCategory.Residential, _cts.Token);
            labels.AddRange(residentials
                .Where(o => residentialIds.Contains(o.Id))
                .Select(o => o.TypeName ?? ""));
        }

        var commercialIds = new HashSet<Guid>(eligibilityCheck.Commercials);
        if (commercialIds.Count > 0)
        {
            var commercials = await commonRepository.GetFloodImpactsByCategory(FloodImpactCategory.Commercial, _cts.Token);
            labels.AddRange(commercials
                .Where(o => commercialIds.Contains(o.Id))
                .Select(o => o.TypeName ?? ""));
        }

        return labels;
    }

    private async Task<IReadOnlyCollection<string>> GetFloodSources(EligibilityCheckDto eligibilityCheck)
    {
        var floodProblems = await commonRepository.GetFloodProblemsByCategory(FloodProblemCategory.PrimaryCause, _cts.Token);
        if (floodProblems.Count == 0)
        {
            return [];
        }

        var query = floodProblems
            .Where(o => eligibilityCheck.Sources.Contains(o.Id))
            .Select(o => o.TypeName ?? "");

        return [.. query];
    }

    private async Task<IReadOnlyCollection<string>> GetFloodSecondarySources(EligibilityCheckDto eligibilityCheck)
    {
        var floodProblems = await commonRepository.GetFloodProblemsByCategory(FloodProblemCategory.SecondaryCause, _cts.Token);
        if (floodProblems.Count == 0)
        {
            return [];
        }

        var query = floodProblems
            .Where(o => eligibilityCheck.SecondarySources.Contains(o.Id))
            .Select(o => o.TypeName ?? "");

        return [.. query];
    }

    private string VulnerablePeopleMessage()
    {
        if (Model.VulnerablePeopleId == null)
        {
            return "Unknown";
        }

        var id = Model.VulnerablePeopleId.Value;
        if (id == Database.Models.Status.RecordStatusIds.No)
        {
            return "No";
        }

        if (id == Database.Models.Status.RecordStatusIds.NotSure)
        {
            return "Not sure";
        }

        // Yes
        var number = Model.NumberOfVulnerablePeople;
        if (number == null)
        {
            return "Unknown";
        }

        var numberText = number == 1 ? "person" : "people";
        return $"Yes - {number} vulnerable {numberText}";
    }
}

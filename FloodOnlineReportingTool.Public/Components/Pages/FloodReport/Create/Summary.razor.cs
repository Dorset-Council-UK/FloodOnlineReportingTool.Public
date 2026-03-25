using FloodOnlineReportingTool.Database.Models.Eligibility;
using FloodOnlineReportingTool.Database.Models.Flood;
using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.Public.Models;
using FloodOnlineReportingTool.Public.Models.Order;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport.Create;

public partial class Summary(
    ILogger<Summary> logger,
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
    //private ExtraData? _createExtraData;
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
            //_createExtraData = await GetCreateExtraData();

            _isLoading = false;
            StateHasChanged();





            //// Set any previously entered data
            //var createExtraData = await GetCreateExtraData();
            //Model.PropertyTypeName = await GetPropertyTypeName(createExtraData);

            //var eligibilityCheck = await GetEligibilityCheck();
            //Model.IsAddress = eligibilityCheck.IsAddress;
            //Model.AddressPreview = eligibilityCheck.LocationDesc;
            //Model.TemporaryAddressPreview = eligibilityCheck.TemporaryLocationDesc;
            //Model.FloodedAreas = await GetFloodedAreas(eligibilityCheck);
            //Model.FloodSources = await GetFloodSources(eligibilityCheck);
            //bool runoff = eligibilityCheck.Sources.Any(s => s == PrimaryCauseIds.RainwaterFlowingOverTheGround);
            //Model.FloodSecondarySources = runoff ? await GetFloodSecondarySources(eligibilityCheck) : null;
            //Model.IsUninhabitable = eligibilityCheck.Uninhabitable;
            //Model.StartDate = eligibilityCheck.ImpactStart;
            //Model.IsOnGoing = eligibilityCheck.OnGoing;
            //Model.FloodDurationKnownId = eligibilityCheck.DurationKnownId;
            //Model.VulnerablePeopleId = eligibilityCheck.VulnerablePeopleId;
            //Model.NumberOfVulnerablePeople = eligibilityCheck.VulnerableCount;

            //// Build the flood lasted for message
            //Model.FloodingLasted = null;
            //var durationId = eligibilityCheck.DurationKnownId;
            //if (!eligibilityCheck.OnGoing && durationId != null)
            //{
            //    if (durationId.Value == FloodDurationIds.DurationKnown && eligibilityCheck.ImpactDuration != null)
            //    {
            //        var duration = TimeSpan.FromHours(eligibilityCheck.ImpactDuration.Value);
            //        Model.FloodingLasted = duration.GdsReadable();
            //    }
            //    else
            //    {
            //        var floodDuration = await commonRepository.GetFloodProblemByCategory(FloodProblemCategory.Duration, durationId.Value, _cts.Token);
            //        Model.FloodingLasted = floodDuration?.TypeDescription;
            //    }
            //}


 
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

    //private async Task<ExtraData> GetCreateExtraData()
    //{
    //    var data = await protectedSessionStorage.GetAsync<ExtraData>(SessionConstants.EligibilityCheck_ExtraData);
    //    if (data.Success)
    //    {
    //        if (data.Value != null)
    //        {
    //            return data.Value;
    //        }
    //    }

    //    logger.LogWarning("Eligibility Check > Extra Data was not found in the protected storage.");
    //    return new();
    //}
}

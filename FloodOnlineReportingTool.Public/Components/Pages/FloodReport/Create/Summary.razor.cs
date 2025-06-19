using FloodOnlineReportingTool.DataAccess.Models;
using FloodOnlineReportingTool.DataAccess.Repositories;
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
    NavigationManager navigationManager,
    IGdsJsInterop gdsJs
) : IPageOrder, IAsyncDisposable
{
    public string Title { get; set; } = FloodReportCreatePages.Summary.Title;
    public IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [
        GeneralPages.Home.ToGdsBreadcrumb(),
        FloodReportPages.Home.ToGdsBreadcrumb(),
        FloodReportCreatePages.FloodSource.ToGdsBreadcrumb(),
    ];

    private Models.FloodReport.Create.Summary Model { get; set; } = default!;

    private EditContext _editContext = default!;
    private ValidationMessageStore _messageStore = default!;
    private readonly CancellationTokenSource _cts = new();
    private bool _isLoading = true;

    protected override void OnInitialized()
    {
        // Setup model and edit context
        Model ??= new();
        _editContext = new(Model);
        _messageStore = new(_editContext);
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
            var createExtraData = await GetCreateExtraData();
            Model.PropertyTypeName = await GetPropertyTypeName(createExtraData);

            var eligibilityCheck = await GetEligibilityCheck();
            Model.AddressPreview = eligibilityCheck.LocationDesc;
            Model.FloodedAreas = await GetFloodedAreas(eligibilityCheck);
            Model.FloodSources = await GetFloodSources(eligibilityCheck);
            Model.IsUninhabitable = eligibilityCheck.Uninhabitable;
            Model.StartDate = eligibilityCheck.ImpactStart;
            Model.IsOnGoing = eligibilityCheck.OnGoing;
            Model.FloodDurationHours = eligibilityCheck.ImpactDuration;
            Model.VulnerablePeopleId = eligibilityCheck.VulnerablePeopleId;
            Model.NumberOfVulnerablePeople = eligibilityCheck.VulnerableCount;

            _isLoading = false;
            StateHasChanged();

            await gdsJs.InitGds(_cts.Token);
        }
    }

    private async Task OnSubmit()
    {
        if (!_editContext.Validate())
        {
            return;
        }

        var saveResult = await SaveEligibilityCheck();
        if (saveResult.Failed)
        {
            _messageStore.Add(_editContext.Field(nameof(Model.AddressPreview)), saveResult.ErrorMessage);
            return;
        }

        var parameters = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            { "Reference", saveResult.Reference },
        };
        var confirmationUrl = navigationManager.GetUriWithQueryParameters(FloodReportCreatePages.Confirmation.Url, parameters.AsReadOnly());
        navigationManager.NavigateTo(confirmationUrl);
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

    private async Task<SaveResult> SaveEligibilityCheck()
    {
        try
        {
            // Create or update the flood report
            var floodReport = await CreateFloodReport();

            if (floodReport is null)
            {
                logger.LogError("Failed to create or update the flood report");
                return SaveResult.Failure("Sorry there was a problem saving your flood report. Please try again but if this issue happens again then please report a bug.");
            }

            // Clear the session data
            await protectedSessionStorage.DeleteAsync(SessionConstants.EligibilityCheck_ExtraData);
            await protectedSessionStorage.DeleteAsync(SessionConstants.EligibilityCheck);

            return SaveResult.Success(floodReport.Reference);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "There was a problem creating the flood report or saving the eligibility check");
            return SaveResult.Failure("Sorry there was a problem saving your flood report. Please try again but if this issue happens again then please report a bug.");
        }
    }

    /// <summary>
    /// Add the flood report, and eligibility check
    /// </summary>
    private async Task<DataAccess.Models.FloodReport?> CreateFloodReport()
    {
        var eligibilityCheck = await GetEligibilityCheck();
        var floodReport = await floodReportRepository.CreateWithEligiblityCheck(eligibilityCheck, _cts.Token);

        if (floodReport is null)
        {
            logger.LogError("Failed to create a new flood report.");
            return null;
        }

        if (floodReport.EligibilityCheck is null)
        {
            logger.LogError("Failed to create eligibility check for flood report reference {Reference}", floodReport.Reference);
            return null;
        }

        return floodReport;
    }

    private string VulnerablePeopleMessage()
    {
        if (Model.VulnerablePeopleId == null)
        {
            return "Unknown";
        }

        var id = Model.VulnerablePeopleId.Value;
        if (id == RecordStatusIds.No)
        {
            return "No";
        }

        if (id == RecordStatusIds.NotSure)
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

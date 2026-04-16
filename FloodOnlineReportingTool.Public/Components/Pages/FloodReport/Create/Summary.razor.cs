using FloodOnlineReportingTool.Database.Models.Eligibility;
using FloodOnlineReportingTool.Database.Models.Flood;
using FloodOnlineReportingTool.Database.Models.Status;
using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.Public.Models;
using FloodOnlineReportingTool.Public.Models.FloodReport.Create;
using FloodOnlineReportingTool.Public.Models.Order;
using FluentValidation;
using FluentValidation.Results;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Globalization;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport.Create;

public partial class Summary(
    ILogger<Summary> logger,
    ICommonRepository commonRepository,
    IFloodReportRepository floodReportRepository,
    IValidator<EligibilityCheckDto> validator,
    ProtectedSessionStorage protectedSessionStorage,
    NavigationManager navigationManager
) : IAsyncDisposable
{
    private readonly IReadOnlyCollection<GdsBreadcrumb> _breadcrumbs = [
        GeneralPages.Home.ToGdsBreadcrumb(),
        FloodReportPages.Home.ToGdsBreadcrumb(),
        FloodReportCreatePages.FloodSource.ToGdsBreadcrumb(),
    ];

    [PersistentState(AllowUpdates = true)]
    public IReadOnlyCollection<FloodProblem>? EligibilityCheckFloodProblems { get; set; }

    [PersistentState(AllowUpdates = true)]
    public IReadOnlyCollection<FloodImpact>? EligibilityCheckFloodImpacts { get; set; }

    [PersistentState(AllowUpdates = true)]
    public string? Yes { get; set; }

    [PersistentState(AllowUpdates = true)]
    public string? No { get; set; }

    [PersistentState(AllowUpdates = true)]
    public string? NotSure { get; set; }

    private EligibilityCheckDto? _eligibilityCheckDto;
    private ExtraData? _extraData;
    private string? _propertyTypeLabel;
    private string[] _floodedAreaLabels = [];
    private string? _isUninhabitableLabel;
    private string? _floodingLastedLabel;
    private string? _vulnerablePeopleLabel;
    private string[] _sourceLabels = [];
    private string[] _secondarySourceLabels = [];
    private readonly CancellationTokenSource _cts = new();
    private bool _isLoading = true;
    private List<ValidationFailure> _validationFailures = [];

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

    protected override async Task OnInitializedAsync()
    {
        // Load persisted lookup data and avoid additional pre-render database calls.
        EligibilityCheckFloodProblems ??= await GetEligibilityCheckFloodProblems();
        EligibilityCheckFloodImpacts ??= await GetEligibilityCheckFloodImpacts();
        if (Yes is null || No is null || NotSure is null)
        {
            (Yes, No, NotSure) = await GetYesNoNotSure();
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _eligibilityCheckDto = await GetEligibilityCheckDto();
            _extraData = await GetCreateExtraData();

            _propertyTypeLabel = EligibilityCheckFloodImpacts?.FirstOrDefault(fi => fi.Id.Equals(_extraData.PropertyType))?.TypeName;
            _floodedAreaLabels = GetFloodedAreas();
            _isUninhabitableLabel = GetIsUninhabitable();
            _floodingLastedLabel = GetFloodingLasted();
            _vulnerablePeopleLabel = GetVulnerablePeople();
            _sourceLabels = GetSources();
            _secondarySourceLabels = GetSecondarySources();

            _isLoading = false;
            await Validate();
            StateHasChanged();
        }
    }

    private async Task OnAcceptAndSend()
    {
        if (_validationFailures.Count > 0)
        {
            return;
        }

        _validationFailures = [];

        if (_eligibilityCheckDto is null)
        {
            logger.LogError("Flood report information was not found");
            _validationFailures.Add(new ValidationFailure("accept", "Sorry there was a problem with your flood report information. Please try again but if this issue happens again then please report a bug."));
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
            var viewUri = new Uri($"{navigationManager.BaseUri}{FloodReportPages.Overview.Url}");
            var floodReport = await floodReportRepository.CreateWithEligiblityCheck(dto, viewUri, _cts.Token);

            if (floodReport.EligibilityCheck is null)
            {
                logger.LogError("Failed to create eligibility check for flood report reference {Reference}", floodReport.Reference);
                _validationFailures.Add(new ValidationFailure("save", saveErrorMessage));
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
            _validationFailures.Add(new ValidationFailure("save", saveErrorMessage));
        }
    }

    private async Task<EligibilityCheckDto> GetEligibilityCheckDto()
    {
        var data = await protectedSessionStorage.GetAsync<EligibilityCheckDto>(SessionConstants.EligibilityCheck);
        if (data.Success && data.Value is not null)
        {
            return data.Value;
        }

        logger.LogWarning("Eligibility Check was not found in the protected storage.");
        return new EligibilityCheckDto();
    }

    private async Task<ExtraData> GetCreateExtraData()
    {
        var data = await protectedSessionStorage.GetAsync<ExtraData>(SessionConstants.EligibilityCheck_ExtraData);
        if (data.Success && data.Value is not null)
        {
            return data.Value;
        }

        logger.LogWarning("Eligibility Check > Extra Data was not found in the protected storage.");
        return new();
    }

    /// <summary>
    /// Get all the flood problems used in eligibility checks, one call makes it more efficient
    /// </summary>
    private async Task<IReadOnlyCollection<FloodProblem>> GetEligibilityCheckFloodProblems()
    {
        string[] categories = [
            FloodProblemCategory.Duration,
            FloodProblemCategory.PrimaryCause,
            FloodProblemCategory.SecondaryCause,
        ];
        var floodProblems = await commonRepository.GetFloodProblemsByCategories(categories, _cts.Token);
        if (floodProblems.Count == 0)
        {
            logger.LogError("There were no flood problems found.");
        }
        return [.. floodProblems];
    }

    /// <summary>
    /// Get all the flood impacts used in eligibility checks, one call makes it more efficient
    /// </summary>
    private async Task<IReadOnlyCollection<FloodImpact>> GetEligibilityCheckFloodImpacts()
    {
        string[] categories = [
            FloodImpactCategory.Commercial,
            FloodImpactCategory.PropertyType,
            FloodImpactCategory.Residential,
        ];
        var floodImpacts = await commonRepository.GetFloodImpactsByCategories(categories, _cts.Token);
        if (floodImpacts.Count == 0)
        {
            logger.LogError("There were no flood impacts found.");
        }
        return [.. floodImpacts];
    }

    /// <summary>
    /// Get the database text for Yes, No, and Not sure, one call makes it more efficient.
    /// </summary>
    private async Task<(string Yes, string No, string NotSure)> GetYesNoNotSure()
    {
        IList<RecordStatus> recordStatuses = await commonRepository.GetRecordStatusesByCategory(RecordStatusCategory.General, _cts.Token);
        if (recordStatuses.Count == 0)
        {
            logger.LogError("There were no record status found.");
            return ("Yes", "No", "Not sure");
        }

        string yes = recordStatuses.FirstOrDefault(rs => rs.Id.Equals(RecordStatusIds.Yes))?.Text ?? "Yes";
        string no = recordStatuses.FirstOrDefault(rs => rs.Id.Equals(RecordStatusIds.No))?.Text ?? "No";
        string notSure = recordStatuses.FirstOrDefault(rs => rs.Id.Equals(RecordStatusIds.NotSure))?.Text ?? "Not sure";
        return (yes, no, notSure);
    }

    private async Task Validate()
    {
        _validationFailures = [];

        if (_eligibilityCheckDto is null)
        {
            logger.LogError("Eligibility Check DTO was not found for validation.");
            return;
        }

        var validationResult = await validator.ValidateAsync(_eligibilityCheckDto, _cts.Token);
        _validationFailures = validationResult.Errors;
    }

    private string[] GetFloodedAreas()
    {
        if (_eligibilityCheckDto is null
            || EligibilityCheckFloodImpacts is null
            || EligibilityCheckFloodImpacts.Count == 0)
        {
            return [];
        }

        // get the flood impact ids
        HashSet<Guid> floodImpactIds = [];
        foreach (var id in _eligibilityCheckDto.Residentials)
        {
            floodImpactIds.Add(id);
        }
        foreach (var id in _eligibilityCheckDto.Commercials)
        {
            floodImpactIds.Add(id);
        }

        if (floodImpactIds.Count == 0)
        {
            return [];
        }

        // get the unique, sorted, flood impact labels
        return [.. EligibilityCheckFloodImpacts
            .Where(o => floodImpactIds.Contains(o.Id))
            .DistinctBy(o => o.TypeName)
            .OrderBy(o => o.TypeName, StringComparer.OrdinalIgnoreCase)
            .Select(o => o.TypeName ?? "Unknown"),
        ];
    }

    private string? GetIsUninhabitable()
    {
        if (_eligibilityCheckDto is null)
        {
            return null;
        }

        return _eligibilityCheckDto.Uninhabitable switch
        {
            true => "You had to evacuate the property as a result of the flooding",
            false => "The property was not evacuated",
            null => null,
        };
    }

    private string? GetFloodingLasted()
    {
        if (_eligibilityCheckDto is null)
        {
            return null;
        }

        if (_eligibilityCheckDto.OnGoing || _eligibilityCheckDto.ImpactDuration is null)
        {
            return null;
        }

        var duration = TimeSpan.FromHours(_eligibilityCheckDto.ImpactDuration.Value);
        return duration.GdsReadable();
    }

    private string? GetVulnerablePeople()
    {
        if (_eligibilityCheckDto is null)
        {
            return null;
        }

        Guid vulnerableId = _eligibilityCheckDto.VulnerablePeopleId;
        int? vulnerableCount = _eligibilityCheckDto.VulnerableCount;

        return vulnerableId switch
        {
            var id when id == RecordStatusIds.No => No,
            var id when id == RecordStatusIds.NotSure => NotSure,
            var id when id == RecordStatusIds.Yes && vulnerableCount is not null =>
                string.Format(CultureInfo.CurrentCulture, "Yes - {0} vulnerable {1}", vulnerableCount.Value, vulnerableCount.Value == 1 ? "person" : "people"),
            _ => null,
        };
    }

    private string[] GetSources()
    {
        if (_eligibilityCheckDto is null
            || EligibilityCheckFloodProblems is null
            || EligibilityCheckFloodProblems.Count == 0)
        {
            return [];
        }

        return [.. EligibilityCheckFloodProblems
            .Where(o => _eligibilityCheckDto.Sources.Contains(o.Id))
            .Select(o => o.TypeName ?? "Unknown"),
        ];
    }

    private string[] GetSecondarySources()
    {
        if (_eligibilityCheckDto is null
            || EligibilityCheckFloodProblems is null
            || EligibilityCheckFloodProblems.Count == 0)
        {
            return [];
        }

        return [.. EligibilityCheckFloodProblems
            .Where(o => _eligibilityCheckDto.SecondarySources.Contains(o.Id))
            .Select(o => o.TypeName ?? "Unknown"),
        ];
    }
}

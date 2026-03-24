using FloodOnlineReportingTool.Database.Models.Eligibility;
using FloodOnlineReportingTool.Database.Models.Flood;
using FloodOnlineReportingTool.Database.Repositories;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Components;

namespace FloodOnlineReportingTool.Public.Components;

public partial class EligibilityCheckDtoSummary(
    ILogger<EligibilityCheckDtoSummary> logger,
    ICommonRepository commonRepository,
    IValidator<EligibilityCheckDto> validator
) : IAsyncDisposable
{
    [Parameter, EditorRequired]
    public EligibilityCheckDto EligibilityCheckDto { get; set; }
    private EligibilityCheckDto? _previousEligibilityCheckDto;

    [Parameter]
    public EventCallback<bool> ValidationStatusChanged { get; set; }
    private List<ValidationFailure> _validationFailures = [];

    [PersistentState(AllowUpdates = true)]
    public IReadOnlyCollection<FloodProblem>? EligibilityCheckFloodProblems { get; set; }

    [PersistentState(AllowUpdates = true)]
    public IReadOnlyCollection<FloodImpact>? EligibilityCheckFloodImpacts { get; set; }

    // Is address / Is postal address
    [Parameter]
    public bool ShowIsAddress { get; set; } = true;
    private string? _isAddressLabel;

    // Location description
    [Parameter]
    public bool ShowLocationDescription { get; set; } = true;
    private string? _locationDescriptionLabel;
    
    // Property type
    [Parameter]
    public bool ShowPropertyType { get; set; } = true;
    private string? _propertyTypeLabel;

    // Flooded areas
    [Parameter]
    public bool ShowFloodedAreas { get; set; } = true;
    private string[] _floodedAreaLabels = [];

    // Is uninhabitable
    [Parameter]
    public bool ShowIsUninhabitable { get; set; } = true;
    private bool? _isUninhabitable;

    // Temporary location description
    [Parameter]
    public bool ShowTemporaryLocationDescription { get; set; } = true;
    private string? _temporaryLocationDescriptionLabel;

    // Impact start
    [Parameter]
    public bool ShowImpactStart { get; set; } = true;
    private string? _impactStartLabel;

    // Is on going
    [Parameter]
    public bool ShowIsOnGoing { get; set; } = true;
    private string? _isOnGoingLabel;

    // Flooding lasted
    [Parameter]
    public bool ShowFloodingLasted { get; set; } = true;
    private string? _floodingLastedLabel;

    // Vulnerable people
    [Parameter]
    public bool ShowVulnerablePeople { get; set; } = true;
    private string? _vulnerablePeopleLabel;

    // Sources
    [Parameter]
    public bool ShowSources { get; set; } = true;
    private string[] _sourceLabels = [];

    // Secondary sources
    [Parameter]
    public bool ShowSecondarySources { get; set; } = true;
    private string[] _secondarySourceLabels = [];

    private readonly CancellationTokenSource _cts = new();
    const string Unknown = "Unknown";

    protected override async Task OnInitializedAsync()
    {
        EligibilityCheckFloodProblems ??= await GetEligibilityCheckFloodProblems();
        EligibilityCheckFloodImpacts ??= await GetEligibilityCheckFloodImpacts();
    }

    protected override async Task OnParametersSetAsync()
    {
        var parametersUnchanged = ReferenceEquals(_previousEligibilityCheckDto, EligibilityCheckDto);
        if (parametersUnchanged)
        {
            return;
        }
        _previousEligibilityCheckDto = EligibilityCheckDto;

        // Property details
        GetIsAddress();
        GetLocationDescription();
        GetPropertyType();
        GetFloodedAreas();
        GetIsUninhabitable();
        GetTemporaryLocationDescription();

        // Flood details
        GetImpactStart();
        GetIsOnGoing();
        GetFloodingLasted();
        GetVulnerablePeople();
        GetSources();
        GetSecondarySources();

        await ValidateAsync();
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
            // Ignore any exceptions that occur during disposal
        }
        GC.SuppressFinalize(this);
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

    private async Task ValidateAsync()
    {
        var validationResult = await validator.ValidateAsync(EligibilityCheckDto, _cts.Token);
        _validationFailures = validationResult.Errors;

        await ValidationStatusChanged.InvokeAsync(validationResult.IsValid);
    }

    private string FloodProblemLabel(Guid? id)
    {
        if (EligibilityCheckFloodProblems is null || EligibilityCheckFloodProblems.Count == 0 || id is null)
        {
            return Unknown;
        }

        return EligibilityCheckFloodProblems
            .Where(o => o.Id == id)
            .Select(o => o.TypeName ?? Unknown)
            .FirstOrDefault(Unknown);
    }

    private string[] FloodProblemLabels(IList<Guid> ids)
    {
        if (EligibilityCheckFloodProblems is null || EligibilityCheckFloodProblems.Count == 0 || ids.Count == 0)
        {
            return [Unknown];
        }

        return [.. EligibilityCheckFloodProblems
            .Where(o => ids.Contains(o.Id))
            .Select(o => o.TypeName ?? Unknown),
        ];
    }

    private string FloodImpactLabel(Guid? id)
    {
        if (EligibilityCheckFloodImpacts is null || EligibilityCheckFloodImpacts.Count == 0 || id is null)
        {
            return Unknown;
        }

        return EligibilityCheckFloodImpacts
            .Where(o => o.Id == id)
            .Select(o => o.TypeName ?? Unknown)
            .FirstOrDefault(Unknown);
    }

    private string[] FloodImpactLabels(IList<Guid> ids)
    {
        if (EligibilityCheckFloodImpacts is null || EligibilityCheckFloodImpacts.Count == 0 || ids.Count == 0)
        {
            return [Unknown];
        }

        return [.. EligibilityCheckFloodImpacts
            .Where(o => ids.Contains(o.Id))
            .Select(o => o.TypeName ?? Unknown),
        ];
    }
}

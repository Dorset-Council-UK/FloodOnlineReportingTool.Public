using FloodOnlineReportingTool.Database.Models.Eligibility;
using FloodOnlineReportingTool.Database.Models.Flood;
using FloodOnlineReportingTool.Database.Models.Status;
using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.Public.Models.Order;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace FloodOnlineReportingTool.Public.Components;

public partial class EligibilityCheckSummary(
    ILogger<EligibilityCheckSummary> logger,
    ICommonRepository commonRepository,
    IValidator<EligibilityCheck> validatorEntity,
    IValidator<EligibilityCheckDto> validatorDto
) : IAsyncDisposable
{
    [Parameter]
    public EligibilityCheckDto? Dto { get; set; }

    [Parameter]
    public EligibilityCheck? Entity { get; set; }

    [Parameter]
    public EventCallback<bool> ValidationStatusChanged { get; set; }
    private List<ValidationFailure> _validationFailures = [];

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

    // Id
    [Parameter]
    public bool ShowId { get; set; } = false;
    private string? _idLabel;

    // Created date
    [Parameter]
    public bool ShowCreatedDate { get; set; } = true;
    private string? _createdDateLabel;

    // Updated date
    [Parameter]
    public bool ShowUpdatedDate { get; set; } = true;
    private string? _updatedDateLabel;

    // Terms agreed
    [Parameter]
    public bool ShowTermsAgreed { get; set; } = true;
    private string? _termsAgreedLabel;

    // Is address / Is postal address
    [Parameter]
    public bool ShowIsAddress { get; set; } = true;
    private string? _isAddressLabel;

    // Location description
    [Parameter]
    public bool ShowLocationDescription { get; set; } = true;
    private string? _locationDescriptionLabel;
    private PageInfo? _locationDescriptionPageInfo;
    private string? _locationDescriptionVisuallyHiddenText;
    
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
    private string? _isUninhabitableLabel;

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
    private const string Unknown = "Unknown";

    protected override async Task OnInitializedAsync()
    {
        // Load persisted lookup data and avoid duplicate pre-render database calls.
        EligibilityCheckFloodProblems ??= await GetEligibilityCheckFloodProblems();
        EligibilityCheckFloodImpacts ??= await GetEligibilityCheckFloodImpacts();
        if (Yes is null || No is null || NotSure is null)
        {
            (Yes, No, NotSure) = await GetYesNoNotSure();
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        if (Dto is null && Entity is null)
        {
            throw new InvalidOperationException("EligibilityCheckSummary component requires either a Dto or an Entity parameter.");
        }

        if (Dto is not null && Entity is not null)
        {
            throw new InvalidOperationException("EligibilityCheckSummary component shouldn't have both Dto and Entity parameters set.");
        }

        // Eligibility check details
        GetId();
        GetCreatedDate();
        GetUpdatedDate();
        GetTermsAgreed();

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

    private async Task ValidateAsync()
    {
        ValidationResult? validationResult;
        if (Entity is not null)
        {
            validationResult = await validatorEntity.ValidateAsync(Entity, _cts.Token);
        }
        else if (Dto is not null)
        {
            validationResult = await validatorDto.ValidateAsync(Dto, _cts.Token);
        }
        else
        {
            return;
        }

        _validationFailures = validationResult.Errors;

        await ValidationStatusChanged.InvokeAsync(validationResult.IsValid);
    }

    private void GetId()
    {
        if (!ShowId || Entity is null)
        {
            _idLabel = null;
            return;
        }
        
        _idLabel = Entity.Id.ToString();
    }

    private void GetCreatedDate()
    {
        if (!ShowCreatedDate || Entity is null)
        {
            _createdDateLabel = null;
            return;
        }
        
        _createdDateLabel = Entity.CreatedUtc.GdsReadable();
    }

    private void GetUpdatedDate()
    {
        if (!ShowUpdatedDate || Entity is null)
        {
            _updatedDateLabel = null;
            return;
        }

        if (Entity.UpdatedUtc is null)
        {
            _updatedDateLabel = Unknown;
            return;
        }

        _updatedDateLabel = Entity.UpdatedUtc.Value.GdsReadable();
    }

    private void GetTermsAgreed()
    {
        if (!ShowTermsAgreed || Entity is null)
        {
            _termsAgreedLabel = null;
            return;
        }

        _termsAgreedLabel = Entity.TermsAgreed.GdsReadable();
    }

    private void GetIsAddress()
    {
        if (!ShowIsAddress)
        {
            _isAddressLabel = null;
            return;
        }

        _isAddressLabel = (Entity?.IsAddress ?? Dto?.IsAddress) switch
        {
            true => Yes,
            false => No,
            null => Unknown,
        };
    }

    private void GetLocationDescription()
    {
        if (!ShowLocationDescription)
        {
            _locationDescriptionLabel = null;
            _locationDescriptionPageInfo = null;
            _locationDescriptionVisuallyHiddenText = null;
            return;
        }

        string? description = Entity?.LocationDesc ?? Dto?.LocationDesc;
        bool? isAddress = Entity?.IsAddress ?? Dto?.IsAddress;

        _locationDescriptionLabel = description ?? Unknown;
        _locationDescriptionPageInfo = isAddress == true ? FloodReportCreatePages.Address : FloodReportCreatePages.Location;
        _locationDescriptionVisuallyHiddenText = isAddress == true ? " address" : " location";
    }

    private void GetPropertyType()
    {
        if (!ShowPropertyType)
        {
            _propertyTypeLabel = null;
            return;
        }

        _propertyTypeLabel = Unknown;
    }

    private void GetFloodedAreas()
    {
        if (!ShowFloodedAreas)
        {
            _floodedAreaLabels = [];
            return;
        }

        // get the flood impact ids from the entity or dto
        HashSet<Guid> floodImpactIds = [];
        if (Entity is not null)
        {
            foreach (var id in Entity.Residentials.Select(o => o.FloodImpactId))
            {
                floodImpactIds.Add(id);
            }

            foreach (var id in Entity.Commercials.Select(o => o.FloodImpactId))
            {
                floodImpactIds.Add(id);
            }
        }
        else if (Dto is not null)
        {
            foreach (var id in Dto.Residentials)
            {
                floodImpactIds.Add(id);
            }

            foreach (var id in Dto.Commercials)
            {
                floodImpactIds.Add(id);
            }
        }

        if (floodImpactIds.Count == 0)
        {
            _floodedAreaLabels = [];
            return;
        }

        if (EligibilityCheckFloodImpacts is null || EligibilityCheckFloodImpacts.Count == 0)
        {
            _floodedAreaLabels = [Unknown];
            return;
        }

        // get the unique, sorted, flood impact labels
        _floodedAreaLabels = [.. EligibilityCheckFloodImpacts
            .Where(o => floodImpactIds.Contains(o.Id))
            .DistinctBy(o => o.Id)
            .OrderBy(o => o.OptionOrder)
            .Select(o => o.TypeName ?? Unknown),
        ];
    }

    private void GetIsUninhabitable()
    {
        if (!ShowIsUninhabitable)
        {
            _isUninhabitableLabel = null;
            return;
        }

        _isUninhabitableLabel = (Entity?.Uninhabitable ?? Dto?.Uninhabitable) switch
        {
            true => Yes,
            false => No,
            null => Unknown,
        };
    }

    private void GetTemporaryLocationDescription()
    {
        if (!ShowTemporaryLocationDescription)
        {
            _temporaryLocationDescriptionLabel = null;
            return;
        }

        _temporaryLocationDescriptionLabel = Entity?.TemporaryLocationDesc ?? Dto?.TemporaryLocationDesc ?? Unknown;
    }

    private void GetImpactStart()
    {
        if (!ShowImpactStart)
        {
            _impactStartLabel = null;
            return;
        }

        DateTimeOffset? dateTime = Entity?.ImpactStart ?? Dto?.ImpactStart;
        if (dateTime is null)
        {
            _impactStartLabel = Unknown;
            return;
        }

        _impactStartLabel = dateTime.Value.GdsReadable();
    }

    private void GetIsOnGoing()
    {
        if (!ShowIsOnGoing)
        {
            _isOnGoingLabel = null;
            return;
        }

        _isOnGoingLabel = (Entity?.OnGoing ?? Dto?.OnGoing) switch
        {
            true => Yes,
            false => No,
            null => Unknown, // shouldn't be null, but just in case
        };
    }

    private void GetFloodingLasted()
    {
        if (!ShowFloodingLasted)
        {
            _floodingLastedLabel = null;
            return;
        }

        if (Entity is { OnGoing: true, ImpactDuration: not 0 })
        {
            var duration = TimeSpan.FromHours(Entity.ImpactDuration);
            _floodingLastedLabel = duration.GdsReadable();
            return;
        }

        if (Dto is { OnGoing: true, ImpactDuration: not null })
        {
            var value = Dto.ImpactDuration.Value;
            if (value is not 0)
            {
                var duration = TimeSpan.FromHours(value);
                _floodingLastedLabel = duration.GdsReadable();
                return;
            }
        }

        _floodingLastedLabel = Unknown;
    }

    private void GetVulnerablePeople()
    {
        if (!ShowVulnerablePeople)
        {
            _vulnerablePeopleLabel = null;
            return;
        }

        Guid vulnerableId = Guid.Empty;
        int? vulnerableCount = null;

        if (Entity is not null)
        {
            vulnerableId = Entity.VulnerablePeopleId;
            vulnerableCount = Entity.VulnerableCount;

        }
        else if (Dto is not null)
        {
            vulnerableId = Dto.VulnerablePeopleId;
            vulnerableCount = Dto.VulnerableCount;
        }

        _vulnerablePeopleLabel = vulnerableId switch
        {
            var id when id == RecordStatusIds.No => No,
            var id when id == RecordStatusIds.NotSure => NotSure,
            var id when id == RecordStatusIds.Yes && vulnerableCount is not null =>
                string.Format(CultureInfo.CurrentCulture, "Yes - {0} vulnerable {1}", vulnerableCount.Value, vulnerableCount.Value == 1 ? "person" : "people"),
            _ => Unknown,
        };
    }

    private void GetSources()
    {
        if (!ShowSources)
        {
            _sourceLabels = [];
            return;
        }

        if (EligibilityCheckFloodProblems is null || EligibilityCheckFloodProblems.Count == 0)
        {
            _sourceLabels = [Unknown];
            return;
        }

        if (Entity is not null)
        {
            _sourceLabels = [.. EligibilityCheckFloodProblems
                .Where(o => Entity.Sources.Select(s => s.FloodProblemId).Contains(o.Id))
                .Select(o => o.TypeName ?? Unknown),
            ];
        }
        else if (Dto is not null)
        {
            _sourceLabels = [.. EligibilityCheckFloodProblems
                .Where(o => Dto.Sources.Contains(o.Id))
                .Select(o => o.TypeName ?? Unknown),
            ];
        }
        else
        {
            _sourceLabels = [Unknown];
        }
    }

    private void GetSecondarySources()
    {
        if (!ShowSecondarySources)
        {
            _secondarySourceLabels = [];
            return;
        }

        if (EligibilityCheckFloodProblems is null || EligibilityCheckFloodProblems.Count == 0)
        {
            _secondarySourceLabels = [Unknown];
            return;
        }

        if (Entity is not null)
        {
            _secondarySourceLabels = [.. EligibilityCheckFloodProblems
                .Where(o => Entity.SecondarySources.Select(s => s.FloodProblemId).Contains(o.Id))
                .Select(o => o.TypeName ?? Unknown),
            ];
        }
        else if (Dto is not null)
        {
            _secondarySourceLabels = [.. EligibilityCheckFloodProblems
                .Where(o => Dto.SecondarySources.Contains(o.Id))
                .Select(o => o.TypeName ?? Unknown),
            ];
        }
        else
        {
            _secondarySourceLabels = [Unknown];
        }
    }
}

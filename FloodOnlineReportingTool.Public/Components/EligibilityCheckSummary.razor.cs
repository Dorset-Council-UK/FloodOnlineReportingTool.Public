using FloodOnlineReportingTool.Database.Models.Eligibility;
using FloodOnlineReportingTool.Database.Models.Flood;
using FloodOnlineReportingTool.Database.Models.Status;
using FloodOnlineReportingTool.Database.Repositories;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace FloodOnlineReportingTool.Public.Components;

public partial class EligibilityCheckSummary(
    ILogger<EligibilityCheckSummary> logger,
    ICommonRepository commonRepository
) : IAsyncDisposable
{
    [Parameter, EditorRequired]
    public EligibilityCheck Entity { get; set; } = default!;

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

    protected override void OnParametersSet()
    {
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

    private void GetId()
    {
        if (!ShowId)
        {
            _idLabel = null;
            return;
        }
        
        _idLabel = Entity.Id.ToString();
    }

    private void GetCreatedDate()
    {
        if (!ShowCreatedDate)
        {
            _createdDateLabel = null;
            return;
        }
        
        _createdDateLabel = Entity.CreatedUtc.GdsReadable();
    }

    private void GetUpdatedDate()
    {
        if (!ShowUpdatedDate || Entity.UpdatedUtc is null)
        {
            _updatedDateLabel = null;
            return;
        }

        _updatedDateLabel = Entity.UpdatedUtc.Value.GdsReadable();
    }

    private void GetTermsAgreed()
    {
        if (!ShowTermsAgreed)
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

        _isAddressLabel = Entity.IsAddress switch
        {
            true => Yes,
            false => No,
        };
    }

    private void GetLocationDescription()
    {
        if (!ShowLocationDescription)
        {
            _locationDescriptionLabel = null;
            return;
        }

        _locationDescriptionLabel = Entity.LocationDesc;
    }

    private void GetPropertyType()
    {
        if (!ShowPropertyType || EligibilityCheckFloodImpacts is null || EligibilityCheckFloodImpacts.Count == 0)
        {
            _propertyTypeLabel = null;
            return;
        }

        Guid? propertyTypeId = (Entity.Residentials.Count, Entity.Commercials.Count) switch
        {
            ( > 0, 0) => FloodImpactIds.Residential,
            (0, > 0) => FloodImpactIds.Commercial,
            ( > 0, > 0) => FloodImpactIds.PropertyTypeOther,
            _ => null,
        };

        _propertyTypeLabel = propertyTypeId is null
            ? null
            : EligibilityCheckFloodImpacts.FirstOrDefault(fi => fi.Id.Equals(propertyTypeId))?.TypeName;
    }

    private void GetFloodedAreas()
    {
        if (!ShowFloodedAreas
            || EligibilityCheckFloodImpacts is null
            || EligibilityCheckFloodImpacts.Count == 0)
        {
            _floodedAreaLabels = [];
            return;
        }

        // get the flood impact ids from the entity or dto
        HashSet<Guid> floodImpactIds = [];
        foreach (var id in Entity.Residentials.Select(o => o.FloodImpactId))
        {
            floodImpactIds.Add(id);
        }
        foreach (var id in Entity.Commercials.Select(o => o.FloodImpactId))
        {
            floodImpactIds.Add(id);
        }

        if (floodImpactIds.Count == 0)
        {
            _floodedAreaLabels = [];
            return;
        }

        // get the unique, sorted, flood impact labels
        _floodedAreaLabels = [.. EligibilityCheckFloodImpacts
            .Where(o => floodImpactIds.Contains(o.Id))
            .DistinctBy(o => o.TypeName)
            .OrderBy(o => o.TypeName, StringComparer.Ordinal)
            .Select(o => o.TypeName ?? "Unknown"),
        ];
    }

    private void GetIsUninhabitable()
    {
        if (!ShowIsUninhabitable)
        {
            _isUninhabitableLabel = null;
            return;
        }

        _isUninhabitableLabel = Entity.Uninhabitable switch
        {
            true => Yes,
            false => No,
        };
    }

    private void GetTemporaryLocationDescription()
    {
        if (!ShowTemporaryLocationDescription)
        {
            _temporaryLocationDescriptionLabel = null;
            return;
        }

        _temporaryLocationDescriptionLabel = Entity.TemporaryLocationDesc;
    }

    private void GetImpactStart()
    {
        if (!ShowImpactStart || Entity.ImpactStart is null)
        {
            _impactStartLabel = null;
            return;
        }

        _impactStartLabel = Entity.ImpactStart.Value.GdsReadable();
    }

    private void GetIsOnGoing()
    {
        if (!ShowIsOnGoing)
        {
            _isOnGoingLabel = null;
            return;
        }

        _isOnGoingLabel = Entity.OnGoing switch
        {
            true => Yes,
            false => No,
        };
    }

    private void GetFloodingLasted()
    {
        if (!ShowFloodingLasted || Entity.OnGoing || Entity.ImpactDuration == 0)
        {
            _floodingLastedLabel = null;
            return;
        }

        var duration = TimeSpan.FromHours(Entity.ImpactDuration);
        _floodingLastedLabel = duration.GdsReadable();
    }

    private void GetVulnerablePeople()
    {
        if (!ShowVulnerablePeople)
        {
            _vulnerablePeopleLabel = null;
            return;
        }

        Guid vulnerableId = Entity.VulnerablePeopleId;
        int? vulnerableCount = Entity.VulnerableCount;

        _vulnerablePeopleLabel = vulnerableId switch
        {
            var id when id == RecordStatusIds.No => No,
            var id when id == RecordStatusIds.NotSure => NotSure,
            var id when id == RecordStatusIds.Yes && vulnerableCount is not null =>
                string.Format(CultureInfo.CurrentCulture, "Yes - {0} vulnerable {1}", vulnerableCount.Value, vulnerableCount.Value == 1 ? "person" : "people"),
            _ => null,
        };
    }

    private void GetSources()
    {
        if (!ShowSources
            || EligibilityCheckFloodProblems is null
            || EligibilityCheckFloodProblems.Count == 0)
        {
            _sourceLabels = [];
            return;
        }

        var sourceIds = Entity.Sources.Select(s => s.FloodProblemId).ToHashSet();
        _sourceLabels = [.. EligibilityCheckFloodProblems
            .Where(o => sourceIds.Contains(o.Id))
            .Select(o => o.TypeName ?? "Unknown"),
        ];
    }

    private void GetSecondarySources()
    {
        if (!ShowSecondarySources
            || EligibilityCheckFloodProblems is null
            || EligibilityCheckFloodProblems.Count == 0)
        {
            _secondarySourceLabels = [];
            return;
        }

        var secondarySourceIds = Entity.SecondarySources.Select(s => s.FloodProblemId).ToHashSet();
        _secondarySourceLabels = [.. EligibilityCheckFloodProblems
            .Where(o => secondarySourceIds.Contains(o.Id))
            .Select(o => o.TypeName ?? "Unknown"),
        ];
    }
}

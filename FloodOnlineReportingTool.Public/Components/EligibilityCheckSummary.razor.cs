using FloodOnlineReportingTool.Database.Models.Eligibility;
using FloodOnlineReportingTool.Database.Models.Flood;
using FloodOnlineReportingTool.Database.Repositories;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

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
    //private bool HasDto => Dto is not null;

    [Parameter]
    public EligibilityCheck? Entity { get; set; }
    //private bool HasEntity => Entity is not null;
    //private EligibilityCheck? _eligibilityCheck;

    [Parameter]
    public EventCallback<bool> ValidationStatusChanged { get; set; }
    private List<ValidationFailure> _validationFailures = [];

    [PersistentState(AllowUpdates = true)]
    public IReadOnlyCollection<FloodProblem>? EligibilityCheckFloodProblems { get; set; }

    [PersistentState(AllowUpdates = true)]
    public IReadOnlyCollection<FloodImpact>? EligibilityCheckFloodImpacts { get; set; }

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
    const string Unknown = "Unknown";

    protected override async Task OnInitializedAsync()
    {
        EligibilityCheckFloodProblems ??= await GetEligibilityCheckFloodProblems();
        EligibilityCheckFloodImpacts ??= await GetEligibilityCheckFloodImpacts();
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

        //if (HasEntity)
        //{
        //    // use the entity if we have it
        //    _eligibilityCheck = Entity;
        //}
        //else if (HasDto)
        //{
        //    // otherwise create a new eligibility check from the dto.
        //    // ignoring the id, created date, updated date, and terms agreed data, as they are not relevant for a DTO summary
        //    var anyGuid = Guid.Empty;
        //    var anyDate = DateTimeOffset.MinValue;
        //    var impactDuration = await Dto.CalculateImpactDurationHours(contextFromFactory??, _cts.Token);
        //    _eligibilityCheck = Dto.ToCreatedEntity(anyGuid, anyDate, anyDate, impactDuration);
        //}

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

        _updatedDateLabel = Entity.UpdatedUtc.GdsReadable();
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
            true => "Yes",
            false => "No",
            null => Unknown,
        };
    }

    private void GetLocationDescription()
    {
        if (!ShowLocationDescription)
        {
            _locationDescriptionLabel = null;
            return;
        }

        _locationDescriptionLabel = Entity?.LocationDesc ?? Dto?.LocationDesc ?? Unknown;
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
}

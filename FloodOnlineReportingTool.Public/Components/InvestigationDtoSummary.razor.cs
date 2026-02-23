using FloodOnlineReportingTool.Database.Models.Flood;
using FloodOnlineReportingTool.Database.Models.Investigate;
using FloodOnlineReportingTool.Database.Models.Status;
using FloodOnlineReportingTool.Database.Repositories;
using Microsoft.AspNetCore.Components;

namespace FloodOnlineReportingTool.Public.Components;

public partial class InvestigationDtoSummary(
    ILogger<InvestigationDtoSummary> logger,
    ICommonRepository commonRepository
) : IAsyncDisposable
{
    [Parameter, EditorRequired]
    public InvestigationDto InvestigationDto { get; set; }

    [Parameter, EditorRequired]
    public bool IsInternal { get; set; }

    [PersistentState(AllowUpdates = true)]
    public IReadOnlyCollection<FloodProblem>? InvestigationFloodProblems { get; set; }

    [PersistentState(AllowUpdates = true)]
    public IReadOnlyCollection<RecordStatus>? InvestigationRecordStatuses { get; set; }

    [PersistentState(AllowUpdates = true)]
    public IReadOnlyCollection<FloodImpact>? InvestigationFloodImpacts { get; set; }

    [PersistentState(AllowUpdates = true)]
    public IReadOnlyCollection<FloodMitigation>? InvestigationFloodMitigations { get; set; }

    // Water speed
    [Parameter]
    public bool ShowWaterSpeed { get; set; } = true;
    private string? _beginLabel;
    private string? _waterSpeedLabel;
    private string? _appearanceLabel;

    // Internal how / Water entry
    [Parameter]
    public bool ShowInternalHow { get; set; } = true;
    private string[] _entryLabels = [];

    // Internal when
    [Parameter]
    public bool ShowInternalWhen { get; set; } = true;
    public string? _internalWhen;

    // Water destination
    [Parameter]
    public bool ShowWaterDestination { get; set; } = true;
    private string[] _destinationLabels = [];

    // Damaged vehicles
    [Parameter]
    public bool ShowDamagedVehicles { get; set; } = true;
    private string? _vehiclesDamagedMessage;

    // Peak depth
    [Parameter]
    public bool ShowPeakDepth { get; set; } = true;
    private bool _isPeakDepthKnown;
    private string? _peakDepthInsideMessage;
    private string? _peakDepthOutsideMessage;
    private string? _peakDepthNotKnownMessage;

    // Community impacts
    [Parameter]
    public bool ShowCommunityImpacts { get; set; } = true;
    private string[] _communityImpactLabels = [];

    // Blockages
    [Parameter]
    public bool ShowBlockages { get; set; } = true;
    private string? _blockagesKnownProblemsLabel;

    // Actions taken
    [Parameter]
    public bool ShowActionsTaken { get; set; } = true;
    private string[] _actionsTakenLabels = [];

    // Warnings - Help received
    [Parameter]
    public bool ShowHelpReceivedWarnings { get; set; } = true;
    private string[] _helpReceivedLabels = [];

    // Warnings - Before the flooding
    [Parameter]
    public bool ShowBeforeFloodingWarnings { get; set; } = true;
    private string? _registeredWithFloodlineLabel;
    private string? _otherWarningReceivedLabel;

    // Warnings - Sources
    [Parameter]
    public bool ShowWarningSources { get; set; } = true;
    private string[] _warningSourcesLabels = [];

    // Warnings - Floodline
    [Parameter]
    public bool ShowFloodlineWarnings { get; set; } = true;
    private bool _isFloodlineWarning;
    private string? _warningTimelyLabel;
    private string? _warningAppropriateLabel;

    // History
    [Parameter]
    public bool ShowHistory { get; set; } = true;
    private string? _historyOfFloodingLabel;

    private readonly CancellationTokenSource _cts = new();
    const string Unknown = "Unknown";

    protected override async Task OnInitializedAsync()
    {
        if (InvestigationFloodProblems is null)
        {
            InvestigationFloodProblems = await GetInvestigationFloodProblems();
        }
        if (InvestigationRecordStatuses is null)
        {
            InvestigationRecordStatuses = await GetInvestigationRecordStatuses();
        }
        if (InvestigationFloodImpacts is null)
        {
            InvestigationFloodImpacts = await GetInvestigationFloodImpacts();
        }
        if (InvestigationFloodMitigations is null)
        {
            InvestigationFloodMitigations = await GetInvestigationFloodMitigations();
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        GetWaterSpeed();
        GetWaterDestination();
        GetDamagedVehicles();
        GetInternalHow();
        GetInternalWhen();
        GetPeakDepth();
        GetCommunityImpact();
        GetBlockages();
        GetActionsTaken();
        GetHistory();

        // warnings
        GetHelpReceivedWarnings();
        GetBeforeTheFloodingWarnings();
        GetWarningSources();
        GetFloodlineWarnings();
    }

    public async ValueTask DisposeAsync()
    {
        await _cts.CancelAsync();
        _cts.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Get all the flood problems used in investigations, one call makes it more efficient
    /// </summary>
    private async Task<IReadOnlyCollection<FloodProblem>> GetInvestigationFloodProblems()
    {
        string[] categories = [
            FloodProblemCategory.Entry,
            FloodProblemCategory.WaterOnset,
            FloodProblemCategory.Speed,
            FloodProblemCategory.Appearance,
            FloodProblemCategory.Destination,
        ];
        var floodProblems = await commonRepository.GetFloodProblemsByCategories(categories, _cts.Token);
        if (floodProblems.Count == 0)
        {
            logger.LogError("There were no flood problems found.");
        }
        return [.. floodProblems];
    }

    /// <summary>
    /// Get all the record statuses used in investigations, one call makes it more efficient
    /// </summary>
    private async Task<IReadOnlyCollection<RecordStatus>> GetInvestigationRecordStatuses()
    {
        string[] categories = [
            RecordStatusCategory.General,
        ];
        var recordStatuses = await commonRepository.GetRecordStatusesByCategories(categories, _cts.Token);
        if (recordStatuses.Count == 0)
        {
            logger.LogError("There were no record statuses found.");
        }
        return [.. recordStatuses];
    }

    /// <summary>
    /// Get all the flood impacts used in investigations, this is only for community impacts, one call makes it more efficient
    /// </summary>
    private async Task<IReadOnlyCollection<FloodImpact>> GetInvestigationFloodImpacts()
    {
        var floodImpacts = await commonRepository.GetFloodImpactsByCategory(FloodImpactCategory.CommunityImpact, _cts.Token);
        if (floodImpacts.Count == 0)
        {
            logger.LogError("There were no flood impacts found.");
        }
        return [.. floodImpacts];
    }

    /// <summary>
    /// Get all the flood mitigations used in investigations, one call makes it more efficient
    /// </summary>
    private async Task<IReadOnlyCollection<FloodMitigation>> GetInvestigationFloodMitigations()
    {
        string[] categories = [
            FloodMitigationCategory.ActionsTaken,
            FloodMitigationCategory.HelpReceived,
            FloodMitigationCategory.WarningSource,
        ];
        var floodMitigations = await commonRepository.GetFloodMitigationsByCategories(categories, _cts.Token);
        if (floodMitigations.Count == 0)
        {
            logger.LogError("There were no flood mitigations found.");
        }
        return [.. floodMitigations];
    }

    private void GetWaterSpeed()
    {
        if (!ShowWaterSpeed)
        {
            _beginLabel = null;
            _waterSpeedLabel = null;
            _appearanceLabel = null;
            return;
        }

        _beginLabel = FloodProblemLabel(InvestigationDto.BeginId);
        _waterSpeedLabel = FloodProblemLabel(InvestigationDto.WaterSpeedId);
        _appearanceLabel = FloodProblemLabel(InvestigationDto.AppearanceId);
    }

    private void GetWaterDestination()
    {
        if (!ShowWaterDestination)
        {
            _destinationLabels = [];
            return;
        }

        if (InvestigationDto.Destinations.Count == 0)
        {
            _destinationLabels = [Unknown];
            return;
        }

        _destinationLabels = FloodProblemLabels(InvestigationDto.Destinations);
    }

    private void GetDamagedVehicles()
    {
        if (!ShowDamagedVehicles)
        {
            _vehiclesDamagedMessage = null;
            return;
        }

        if (InvestigationDto.WereVehiclesDamagedId is null)
        {
            _vehiclesDamagedMessage = Unknown;
            return;
        }

        if (InvestigationDto.WereVehiclesDamagedId == RecordStatusIds.Yes)
        {
            _vehiclesDamagedMessage = BuildMessage(
                InvestigationDto.NumberOfVehiclesDamaged,
                "{0} vehicle was damaged",
                "{0} vehicles were damaged"
            );
            return;
        }

        _vehiclesDamagedMessage = RecordStatusLabel(InvestigationDto.WereVehiclesDamagedId);
    }

    private void GetInternalHow()
    {
        if (!IsInternal || !ShowInternalHow)
        {
            _entryLabels = [];
            return;
        }

        if (InvestigationDto.Entries.Count == 0)
        {
            _entryLabels = [Unknown];
            return;
        }

        _entryLabels = FloodProblemLabels(InvestigationDto.Entries);
    }

    private void GetInternalWhen()
    {
        if (!IsInternal || !ShowInternalWhen)
        {
            _internalWhen = null;
            return;
        }

        _internalWhen = InvestigationDto.WhenWaterEnteredKnownId switch
        {
            var id when id == RecordStatusIds.Yes => InvestigationDto.FloodInternalUtc.GdsReadable(),
            var id when id == RecordStatusIds.No => RecordStatusLabel(RecordStatusIds.No),
            _ => Unknown,
        };
    }

    private void GetPeakDepth()
    {
        // Reset all peak depth fields
        _isPeakDepthKnown = false;
        _peakDepthNotKnownMessage = null;
        _peakDepthInsideMessage = null;
        _peakDepthOutsideMessage = null;

        if (!ShowPeakDepth)
        {
            return;
        }

        if (InvestigationDto.IsPeakDepthKnownId == RecordStatusIds.Yes)
        {
            _isPeakDepthKnown = true;
            _peakDepthInsideMessage = BuildMessage(InvestigationDto.PeakInsideCentimetres, "{0} centimetre", "{0} centimetres");
            _peakDepthOutsideMessage = BuildMessage(InvestigationDto.PeakOutsideCentimetres, "{0} centimetre", "{0} centimetres");
        }
        else if (InvestigationDto.IsPeakDepthKnownId == RecordStatusIds.No)
        {
            _peakDepthNotKnownMessage = "Not known";
        }
        else
        {
            _peakDepthNotKnownMessage = Unknown;
        }
    }

    private void GetCommunityImpact()
    {
        if (!ShowCommunityImpacts)
        {
            _communityImpactLabels = [];
            return;
        }

        if (InvestigationDto.CommunityImpacts.Count == 0)
        {
            _communityImpactLabels = [Unknown];
            return;
        }

        _communityImpactLabels = FloodImpactLabels(InvestigationDto.CommunityImpacts);
    }

    private void GetBlockages()
    {
        if (!ShowBlockages)
        {
            _blockagesKnownProblemsLabel = null;
            return;
        }

        _blockagesKnownProblemsLabel = InvestigationDto.HasKnownProblems switch
        {
            var hasProblems when hasProblems == true => RecordStatusLabel(RecordStatusIds.Yes),
            var hasProblems when hasProblems == false => RecordStatusLabel(RecordStatusIds.No),
            _ => Unknown,
        };
    }

    private void GetActionsTaken()
    {
        if (!ShowActionsTaken)
        {
            _actionsTakenLabels = [];
            return;
        }

        if (InvestigationDto.ActionsTaken.Count == 0)
        {
            _actionsTakenLabels = [Unknown];
            return;
        }

        _actionsTakenLabels = FloodMitigationLabels(InvestigationDto.ActionsTaken);
    }

    private void GetHelpReceivedWarnings()
    {
        if (!ShowHelpReceivedWarnings)
        {
            _helpReceivedLabels = [];
            return;
        }

        if (InvestigationDto.HelpReceived.Count == 0)
        {
            _helpReceivedLabels = [Unknown];
            return;
        }

        _helpReceivedLabels = FloodMitigationLabels(InvestigationDto.HelpReceived);
    }

    private void GetBeforeTheFloodingWarnings()
    {
        if (!ShowBeforeFloodingWarnings)
        {
            _registeredWithFloodlineLabel = null;
            _otherWarningReceivedLabel = null;
            return;
        }

        _registeredWithFloodlineLabel = RecordStatusLabel(InvestigationDto.FloodlineId);
        _otherWarningReceivedLabel = RecordStatusLabel(InvestigationDto.WarningReceivedId);
    }

    private void GetWarningSources()
    {
        if (!ShowWarningSources)
        {
            _warningSourcesLabels = [];
            return;
        }

        if (InvestigationDto.WarningSources.Count == 0)
        {
            _warningSourcesLabels = [Unknown];
            return;
        }

        _warningSourcesLabels = FloodMitigationLabels(InvestigationDto.WarningSources);
    }

    private void GetFloodlineWarnings()
    {
        // Reset all floodline warning fields
        _isFloodlineWarning = false;
        _warningTimelyLabel = null;
        _warningAppropriateLabel = null;

        if (!ShowFloodlineWarnings)
        {
            return;
        }

        if (InvestigationDto.WarningSources.Contains(FloodMitigationIds.FloodlineWarning))
        {
            _isFloodlineWarning = true;
            _warningTimelyLabel = RecordStatusLabel(InvestigationDto.WarningTimelyId);
            _warningAppropriateLabel = RecordStatusLabel(InvestigationDto.WarningAppropriateId);
        }
    }

    private void GetHistory()
    {
        if (!ShowHistory)
        {
            _historyOfFloodingLabel = null;
            return;
        }

        _historyOfFloodingLabel = RecordStatusLabel(InvestigationDto.HistoryOfFloodingId);
    }


    private string FloodProblemLabel(Guid? id)
    {
        if (InvestigationFloodProblems is null || InvestigationFloodProblems.Count == 0 || id is null)
        {
            return Unknown;
        }

        return InvestigationFloodProblems
            .Where(o => o.Id == id)
            .Select(o => o.TypeName ?? Unknown)
            .FirstOrDefault(Unknown);
    }

    private string[] FloodProblemLabels(IList<Guid> ids)
    {
        if (InvestigationFloodProblems is null || InvestigationFloodProblems.Count == 0 || ids.Count == 0)
        {
            return [Unknown];
        }

        return [.. InvestigationFloodProblems
            .Where(o => ids.Contains(o.Id))
            .Select(o => o.TypeName ?? Unknown),
        ];
    }

    private string RecordStatusLabel(Guid? id)
    {
        if (InvestigationRecordStatuses is null || InvestigationRecordStatuses.Count == 0 || id is null)
        {
            return Unknown;
        }

        return InvestigationRecordStatuses
            .Where(o => o.Id == id)
            .Select(o => o.Text ?? Unknown)
            .FirstOrDefault(Unknown);
    }

    private string[] FloodImpactLabels(IList<Guid> ids)
    {
        if (InvestigationFloodImpacts is null || InvestigationFloodImpacts.Count == 0 || ids.Count == 0)
        {
            return [Unknown];
        }

        return [.. InvestigationFloodImpacts
            .Where(o => ids.Contains(o.Id))
            .Select(o => o.TypeName ?? Unknown),
        ];
    }

    private string[] FloodMitigationLabels(IList<Guid> ids)
    {
        if (InvestigationFloodMitigations is null || InvestigationFloodMitigations.Count == 0 || ids.Count == 0)
        {
            return [Unknown];
        }

        return [.. InvestigationFloodMitigations
            .Where(o => ids.Contains(o.Id))
            .Select(o => o.TypeName ?? Unknown),
        ];
    }

    private static string BuildMessage(int? number, string singularFormat, string pluralFormat)
    {
        if (number is null)
        {
            return Unknown;
        }

        if (number == 1)
        {
            // examples: 1 centimetre,
            return string.Format(singularFormat, number);
        }

        // examples: 0 centimetres, 2 centimetres, 99 centimetres
        return string.Format(pluralFormat, number);
    }
}

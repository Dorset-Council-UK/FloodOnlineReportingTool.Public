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

    [PersistentState]
    public IReadOnlyCollection<FloodProblem>? FloodProblems { get; set; }


    // Water speed
    [Parameter]
    public bool ShowWaterSpeed { get; set; } = true;
    private string _beginLabel = "";
    private string _waterSpeedLabel = "";
    private string _appearanceLabel = "";

    // Water destination
    [Parameter]
    public bool ShowWaterDestination { get; set; } = true;
    private string[] _destinationLabels = [];

    // Damaged vehicles
    [Parameter]
    public bool ShowDamagedVehicles { get; set; } = true;
    private string _vehiclesDamagedMessage = "";




    // Help received warnings
    [Parameter]
    public bool ShowHelpReceivedWarnings { get; set; } = true;
    private string[] _helpReceivedLabels = [];

    // Before the flooding warnings
    public bool ShowBeforeFloodingWarnings { get; set; } = true;
    private string _registeredWithFloodlineLabel = "";
    private string _otherWarningReceivedLabel = "";

    // Floodline warnings
    [Parameter]
    public bool ShowFloodlineWarnings { get; set; } = true;
    private bool _isFloodlineWarning;
    private string _warningTimelyLabel = "";
    private string _warningAppropriateLabel = "";



    private readonly CancellationTokenSource _cts = new();
    
    private IReadOnlyCollection<RecordStatus> _recordStatuses = [];
    private IReadOnlyCollection<FloodMitigation> _floodMitigations = [];

    protected override async Task OnInitializedAsync()
    {
        if (FloodProblems is null)
        {
            FloodProblems = await GetInvestigationFloodProblems();
        }
        _recordStatuses = await GetInvestigationRecordStatuses();
        _floodMitigations = await GetInvestigationFloodMitigations();
    }

    protected override async Task OnParametersSetAsync()
    {
        await WaterSpeed();
        await WaterDestination();
        await DamagedVehicles();
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

    private async Task WaterSpeed()
    {
        if (!ShowWaterSpeed)
        {
            _beginLabel = "";
            _waterSpeedLabel = "";
            _appearanceLabel = "";
            return;
        }

        _beginLabel = FloodProblemLabel(InvestigationDto.BeginId);
        _waterSpeedLabel = FloodProblemLabel(InvestigationDto.WaterSpeedId);
        _appearanceLabel = FloodProblemLabel(InvestigationDto.AppearanceId);
    }

    private async Task WaterDestination()
    {
        if (!ShowWaterDestination)
        {
            _destinationLabels = [];
            return;
        }

        _destinationLabels = FloodProblemLabels(InvestigationDto.Destinations);
    }

    private async Task DamagedVehicles()
    {
        if (!ShowDamagedVehicles || InvestigationDto.WereVehiclesDamagedId is null)
        {
            _vehiclesDamagedMessage = "";
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

    private string FloodProblemLabel(Guid? id)
    {
        if (FloodProblems is null || FloodProblems.Count == 0 || id is null)
        {
            return "";
        }

        return FloodProblems
            .Where(o => o.Id == id)
            .Select(o => o.TypeName ?? "")
            .FirstOrDefault("");
    }

    private string[] FloodProblemLabels(IList<Guid> ids)
    {
        if (FloodProblems is null || FloodProblems.Count == 0 || ids.Count == 0)
        {
            return [];
        }

        return [.. FloodProblems
            .Where(o => ids.Contains(o.Id))
            .Select(o => o.TypeName ?? ""),
        ];
    }

    private string RecordStatusLabel(Guid? id)
    {
        if (id is null)
        {
            return "";
        }

        return _recordStatuses
            .Where(o => o.Id == id)
            .Select(o => o.Text ?? "")
            .FirstOrDefault("");
    }

    private static string BuildMessage(int? number, string singularFormat, string pluralFormat)
    {
        if (number is null)
        {
            return "";
        }

        if (number > 1)
        {
            return string.Format(singularFormat, number);
        }

        return string.Format(pluralFormat, number);
    }

    //private async Task CreateSummary(InvestigationDto dto, bool isInternal)
    //{



    //    // Internal
    //    if (isInternal)
    //    {
    //        Model.IsInternal = true;

    //        // Internal - How it entered - Water entry
    //        Model.EntryLabels = FloodProblemLabels(dto.Entries);

    //        // Internal - When it entered
    //        if (dto.WhenWaterEnteredKnownId == RecordStatusIds.Yes)
    //        {
    //            Model.InternalMessage = dto.FloodInternalUtc.GdsReadable();
    //        }
    //        else if (dto.WhenWaterEnteredKnownId == RecordStatusIds.No)
    //        {
    //            Model.InternalMessage = RecordStatusLabel(RecordStatusIds.No);
    //        }
    //        else
    //        {
    //            Model.InternalMessage = "Unknown";
    //        }
    //    }

    //    // Peak depth
    //    if (dto.IsPeakDepthKnownId != null)
    //    {
    //        Model.IsPeakDepthKnownId = dto.IsPeakDepthKnownId.Value;
    //    }
    //    if (Model.IsPeakDepthKnownId == RecordStatusIds.Yes)
    //    {
    //        Model.PeakDepthInsideMessage = BuildMessage(dto.PeakInsideCentimetres, UnitCentimetres, UnitCentimetre);
    //        Model.PeakDepthOutsideMessage = BuildMessage(dto.PeakOutsideCentimetres, UnitCentimetres, UnitCentimetre);
    //    }
    //    else if (Model.IsPeakDepthKnownId == RecordStatusIds.No)
    //    {
    //        Model.PeakDepthMessage = "Not known";
    //    }
    //    else
    //    {
    //        Model.PeakDepthMessage = "Unknown";
    //    }

    //    // Community impact
    //    Model.CommunityImpactLabels = await GetCommunityImpactLabels(dto.CommunityImpacts);

    //    // Blockages
    //    if (dto.HasKnownProblems != null)
    //    {
    //        Model.HasKnownProblemsMessage = dto.HasKnownProblems.Value
    //            ? RecordStatusLabel(RecordStatusIds.Yes)
    //            : RecordStatusLabel(RecordStatusIds.No);
    //    }

    //    // Actions taken
    //    Model.ActionsTakenLabels = FloodMitigationLabels(dto.ActionsTaken);

    //    // Help received
    //    Model.HelpReceivedLabels = FloodMitigationLabels(dto.HelpReceived);

    //    // Before the flooding - Warnings
    //    Model.RegisteredWithFloodlineLabel = RecordStatusLabel(dto.FloodlineId);
    //    Model.OtherWarningReceivedLabel = RecordStatusLabel(dto.WarningReceivedId);

    //    // Warning sources
    //    Model.WarningSourceLabels = FloodMitigationLabels(dto.WarningSources);

    //    // Floodline warnings
    //    if (dto.WarningSources.Contains(FloodMitigationIds.FloodlineWarning))
    //    {
    //        Model.IsFloodlineWarning = true;
    //        Model.WarningTimelyLabel = RecordStatusLabel(dto.WarningTimelyId);
    //        Model.WarningAppropriateLabel = RecordStatusLabel(dto.WarningAppropriateId);
    //    }

    //    // History
    //    Model.HistoryOfFloodingLabel = RecordStatusLabel(dto.HistoryOfFloodingId);
    //}
}

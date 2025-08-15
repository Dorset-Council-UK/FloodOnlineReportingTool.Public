using FloodOnlineReportingTool.Database.Models;
using FloodOnlineReportingTool.Database.Repositories;
using GdsBlazorComponents;
using FloodOnlineReportingTool.Public.Models;
using FloodOnlineReportingTool.Public.Models.Order;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport.Investigation;

[Authorize]
public partial class Summary(
    ILogger<Summary> logger,
    ICommonRepository commonRepository,
    IEligibilityCheckRepository eligibilityCheckRepository,
    IInvestigationRepository investigationRepository,
    ProtectedSessionStorage protectedSessionStorage,
    NavigationManager navigationManager,
    IGdsJsInterop gdsJs
) : IPageOrder, IAsyncDisposable
{
    // Page order properties
    public string Title { get; set; } = InvestigationPages.Summary.Title;
    public IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [
        GeneralPages.Home.ToGdsBreadcrumb(),
        FloodReportPages.Overview.ToGdsBreadcrumb(),
        InvestigationPages.History.ToGdsBreadcrumb(),
    ];

    [CascadingParameter]
    public Task<AuthenticationState>? AuthenticationState { get; set; }

    private Models.FloodReport.Investigation.Summary Model { get; set; } = default!;

    private EditContext _editContext = default!;
    private ValidationMessageStore _messageStore = default!;
    private readonly CancellationTokenSource _cts = new();
    private bool _isLoading = true;

    private FieldIdentifier errorField;
    private const string UnitCentimetres = "centimetres";
    private const string UnitCentimetre = "centimetre";
    private IReadOnlyCollection<FloodProblem> _floodProblems = [];
    private IReadOnlyCollection<RecordStatus> _recordStatuses = [];
    private IReadOnlyCollection<FloodMitigation> _floodMitigations = [];

    protected override void OnInitialized()
    {
        // Setup model and edit context
        Model ??= new();
        _editContext = new(Model);
        _messageStore = new(_editContext);

        errorField = FieldIdentifier.Create(() => Model.BeginLabel);
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
            var userId = await AuthenticationState.IdentityUserId() ?? Guid.Empty;
            var eligibilityCheck = await eligibilityCheckRepository.ReportedByUser(userId, _cts.Token);
            var isInternal = eligibilityCheck?.IsInternal() == true;

            _floodProblems = await GetInvestigationFloodProblems();
            _recordStatuses = await GetInvestigationRecordStatuses();
            _floodMitigations = await GetInvestigationFloodMitigations();

            var investigation = await GetInvestigation();

            await CreateSummary(investigation, isInternal);

            _editContext.Validate();
            _isLoading = false;
            StateHasChanged();
            _editContext.NotifyValidationStateChanged();

            await gdsJs.InitGds(_cts.Token);
        }
    }

    private async Task CreateSummary(InvestigationDto dto, bool isInternal)
    {
        // Water speed
        Model.BeginLabel = FloodProblemLabel(dto.BeginId);
        Model.WaterSpeedLabel = FloodProblemLabel(dto.WaterSpeedId);
        Model.AppearanceLabel = FloodProblemLabel(dto.AppearanceId);

        // Water destination
        Model.DestinationLabels = FloodProblemLabels(dto.Destinations);

        // Damaged vehicles
        if (dto.WereVehiclesDamagedId != null)
        {
            Model.VehiclesDamagedMessage = dto.WereVehiclesDamagedId == RecordStatusIds.Yes
                ? dto.NumberOfVehiclesDamaged?.ToString() ?? ""
                : RecordStatusLabel(dto.WereVehiclesDamagedId);
        }

        // Internal
        if (isInternal)
        {
            Model.IsInternal = true;

            // Internal - How it entered - Water entry
            Model.EntryLabels = FloodProblemLabels(dto.Entries);

            // Internal - When it entered
            if (dto.WhenWaterEnteredKnownId == RecordStatusIds.Yes)
            {
                Model.InternalMessage = dto.FloodInternalUtc.GdsReadable();
            }
            else if (dto.WhenWaterEnteredKnownId == RecordStatusIds.No)
            {
                Model.InternalMessage = RecordStatusLabel(RecordStatusIds.No);
            }
            else
            {
                Model.InternalMessage = "Unknown";
            }
        }

        // Peak depth
        if (dto.IsPeakDepthKnownId != null)
        {
            Model.IsPeakDepthKnownId = dto.IsPeakDepthKnownId.Value;
        }
        if (Model.IsPeakDepthKnownId == RecordStatusIds.Yes)
        {
            Model.PeakDepthInsideMessage = BuildMessage(dto.PeakInsideCentimetres, UnitCentimetres, UnitCentimetre);
            Model.PeakDepthOutsideMessage = BuildMessage(dto.PeakOutsideCentimetres, UnitCentimetres, UnitCentimetre);
        }
        else if (Model.IsPeakDepthKnownId == RecordStatusIds.No)
        {
            Model.PeakDepthMessage = "Not known";
        }
        else
        {
            Model.PeakDepthMessage = "Unknown";
        }

        // Community impact
        Model.CommunityImpactLabels = await GetCommunityImpactLabels(dto.CommunityImpacts);

        // Blockages
        if (dto.HasKnownProblems != null)
        {
            Model.HasKnownProblemsMessage = dto.HasKnownProblems.Value
                ? RecordStatusLabel(RecordStatusIds.Yes)
                : RecordStatusLabel(RecordStatusIds.No);
        }

        // Actions taken
        Model.ActionsTakenLabels = FloodMitigationLabels(dto.ActionsTaken);

        // Help received
        Model.HelpReceivedLabels = FloodMitigationLabels(dto.HelpReceived);

        // Before the flooding - Warnings
        Model.RegisteredWithFloodlineLabel = RecordStatusLabel(dto.FloodlineId);
        Model.OtherWarningReceivedLabel = RecordStatusLabel(dto.WarningReceivedId);

        // Warning sources
        Model.WarningSourceLabels = FloodMitigationLabels(dto.WarningSources);

        // Floodline warnings
        if (dto.WarningSources.Contains(FloodMitigationIds.FloodlineWarning))
        {
            Model.IsFloodlineWarning = true;
            Model.WarningTimelyLabel = RecordStatusLabel(dto.WarningTimelyId);
            Model.WarningAppropriateLabel = RecordStatusLabel(dto.WarningAppropriateId);
        }

        // History
        Model.HistoryOfFloodingLabel = RecordStatusLabel(dto.HistoryOfFloodingId);
    }

    private async Task OnSubmit()
    {
        if (!_editContext.Validate())
        {
            return;
        }

        await SaveInvestigation();
    }

    private async Task SaveInvestigation()
    {
        logger.LogDebug("Saving investigation information..");
        try
        {
            var userId = await AuthenticationState.IdentityUserId();
            var investigation = await GetInvestigation();
            await investigationRepository.CreateForUser(userId.Value, investigation, _cts.Token);

            // Clear the session data
            await protectedSessionStorage.DeleteAsync(SessionConstants.Investigation);

            logger.LogInformation("Investigation created successfully for user {UserId}", userId);
            navigationManager.NavigateTo(InvestigationPages.Confirmation.Url);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "There was a problem creating the investigation information");
            _messageStore.Add(errorField, $"There was a problem creating the investigation information. Please try again but if this issue happens again then please report a bug.");
            _editContext.NotifyValidationStateChanged();
        }
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

        logger.LogDebug("Eligibility Check was not found in the protected storage.");
        return new();
    }

    private async Task<InvestigationDto> GetInvestigation()
    {
        var data = await protectedSessionStorage.GetAsync<InvestigationDto>(SessionConstants.Investigation);
        if (data.Success)
        {
            if (data.Value != null)
            {
                return data.Value;
            }
        }

        logger.LogWarning("Investigation was not found in the protected storage.");
        return new InvestigationDto();
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
            _messageStore.Add(errorField, "Sorry there was a problem getting the flood problems. Please try again but if this issue happens again then please report a bug.");
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
            _messageStore.Add(errorField, "Sorry there was a problem getting the record statuses. Please try again but if this issue happens again then please report a bug.");
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
            _messageStore.Add(errorField, "Sorry there was a problem getting the flood mitigations. Please try again but if this issue happens again then please report a bug.");
        }
        return [.. floodMitigations];
    }

    private static string BuildMessage(int? depth, string units, string unit)
    {
        if (depth == null)
        {
            return "";
        }

        if (depth > 1)
        {
            return $"{depth} {units} ";
        }

        return $"{depth} {unit} ";
    }

    private string FloodProblemLabel(Guid? id)
    {
        if (id == null)
        {
            return "";
        }

        return _floodProblems
            .Where(o => o.Id == id)
            .Select(o => o.TypeName ?? "")
            .FirstOrDefault("");
    }

    private IReadOnlyCollection<string> FloodProblemLabels(IList<Guid> ids)
    {
        return [.. _floodProblems
            .Where(o => ids.Contains(o.Id))
            .Select(o => o.TypeName ?? ""),
        ];
    }

    private string RecordStatusLabel(Guid? id)
    {
        if (id == null)
        {
            return "";
        }

        return _recordStatuses
            .Where(o => o.Id == id)
            .Select(o => o.Text ?? "")
            .FirstOrDefault("");
    }

    private async Task<IReadOnlyCollection<string>> GetCommunityImpactLabels(IList<Guid> ids)
    {
        var communityImpacts = await commonRepository.GetFloodImpactsByCategory(FloodImpactCategory.CommunityImpact, _cts.Token);
        return [.. communityImpacts
            .Where(o => ids.Contains(o.Id))
            .Select(o => o.TypeName ?? ""),
        ];
    }

    private IReadOnlyCollection<string> FloodMitigationLabels(IList<Guid> ids)
    {
        return [.. _floodMitigations
            .Where(o => ids.Contains(o.Id))
            .Select(o => o.TypeName ?? ""),
        ];
    }
}

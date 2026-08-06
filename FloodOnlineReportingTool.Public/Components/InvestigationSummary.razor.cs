using FloodOnlineReportingTool.Database.Models.Investigate;
using FloodOnlineReportingTool.Database.Repositories;
using Microsoft.AspNetCore.Components;

namespace FloodOnlineReportingTool.Public.Components;

public partial class InvestigationSummary(
    ILogger<EligibilityCheckSummary> logger,
    ICommonRepository commonRepository
) : IAsyncDisposable
{
    [Parameter, EditorRequired]
    public Investigation Entity { get; set; }

    [Parameter]
    public bool ShowWaterSpeed { get; set; } = true;
    private string? _beginLabel;
    private string? _waterSpeedLabel;
    private string? _appearanceLabel;

    [Parameter]
    public bool ShowWaterDestination { get; set; } = true;
    private string[] _destinationLabels = [];

    [Parameter]
    public bool ShowDamagedVehicles { get; set; } = true;
    private string? _vehiclesDamagedMessage;

    [Parameter, EditorRequired]
    public bool IsInternal { get; set; }

    [Parameter]
    public bool ShowInternalHow { get; set; } = true;
    private string[] _entryLabels = [];

    [Parameter]
    public bool ShowInternalWhen { get; set; } = true;
    private string? _internalWhen;

    [Parameter]
    public bool ShowPeakDepth { get; set; } = true;
    private bool _isPeakDepthKnown;
    private string? _peakDepthInsideMessage;
    private string? _peakDepthOutsideMessage;
    private string? _peakDepthNotKnownMessage;

    [Parameter]
    public bool ShowServiceImpacts { get; set; } = true;
    private string[] _serviceImpactLabels = [];

    [Parameter]
    public bool ShowCommunityImpacts { get; set; } = true;
    private string[] _communityImpactLabels = [];

    [Parameter]
    public bool ShowBlockages { get; set; } = true;
    private string? _blockagesKnownProblemsLabel;

    [Parameter]
    public bool ShowActionsTaken { get; set; } = true;
    private string[] _actionsTakenLabels = [];

    [Parameter]
    public bool ShowHistory { get; set; } = true;
    private string? _historyOfFloodingLabel;

    [Parameter]
    public bool ShowInsurance { get; set; } = true;
    private string? _propertyInsuredLabel;

    [Parameter]
    public bool ShowHelpReceivedWarnings { get; set; } = true;
    private string[] _helpReceivedLabels = [];

    [Parameter]
    public bool ShowBeforeFloodingWarnings { get; set; } = true;
    private string? _registeredWithFloodlineLabel;
    private string? _otherWarningReceivedLabel;

    [Parameter]
    public bool ShowWarningSources { get; set; } = true;
    private string[] _warningSourcesLabels = [];

    [Parameter]
    public bool ShowFloodlineWarnings { get; set; } = true;
    private bool _isFloodlineWarning;
    private string? _warningTimelyLabel;
    private string? _warningAppropriateLabel;

    private bool ShowWarnings()
        => ShowHelpReceivedWarnings || ShowBeforeFloodingWarnings || ShowWarningSources || (ShowFloodlineWarnings && _isFloodlineWarning);


    private readonly CancellationTokenSource _cts = new();

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
}

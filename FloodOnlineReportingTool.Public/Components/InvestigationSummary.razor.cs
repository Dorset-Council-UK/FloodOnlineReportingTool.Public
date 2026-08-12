using FloodOnlineReportingTool.Database.Models.Flood;
using FloodOnlineReportingTool.Database.Models.Investigate;
using Microsoft.AspNetCore.Components;

namespace FloodOnlineReportingTool.Public.Components;

public partial class InvestigationSummary : IAsyncDisposable
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
    private const string Unknown = "Unknown";

    protected override void OnParametersSet()
    {
        GetWaterSpeed();
        GetWaterDestination();
        GetDamagedVehicles();
        GetInternalHow();
        GetInternalWhen();
        GetPeakDepth();
        GetServiceImpact();
        GetCommunityImpact();
        GetBlockages();
        GetActionsTaken();
        GetHistory();
        GetInsurance();

        // warnings
        GetHelpReceivedWarnings();
        GetBeforeTheFloodingWarnings();
        GetWarningSources();
        GetFloodlineWarnings();
    }


    private void GetWaterSpeed()
    {
        PopulateLabel(ShowWaterSpeed, ref _beginLabel, e => e.Begin.TypeName);
        PopulateLabel(ShowWaterSpeed, ref _waterSpeedLabel, e => e.WaterSpeed.TypeName);
        PopulateLabel(ShowWaterSpeed, ref _appearanceLabel, e => e.Appearance.TypeName);
    }


    private void GetWaterDestination() 
        => PopulateLabels(ShowWaterDestination, ref _destinationLabels, Entity.Destinations, d => d.FloodProblem.TypeName);


    private void GetDamagedVehicles() 
        => PopulateLabel(ShowDamagedVehicles, ref _vehiclesDamagedMessage, e => e.NumberOfVehiclesDamaged switch
            {
                null or 0 => null,
                1 => "1 vehicle was damaged",
                _ => $"{Entity.NumberOfVehiclesDamaged} vehicles were damaged"
            });


    private void GetInternalHow()
        => PopulateLabels(IsInternal && ShowInternalHow, ref _entryLabels, Entity.Entries, e => e.FloodProblem.TypeName);


    private void GetInternalWhen()
        => PopulateLabel(IsInternal && ShowInternalWhen, ref _internalWhen, e => e.WhenWaterEnteredKnown?.Text);


    private void GetPeakDepth()
    {
        //TODO: Outside depth, due to needing a map to display. 

        // Reset all peak depth fields
        _isPeakDepthKnown = false;
        _peakDepthNotKnownMessage = null;
        _peakDepthInsideMessage = null;
        _peakDepthOutsideMessage = null;

        if (!ShowPeakDepth)
        {
            return;
        }

        if(Entity.IsPeakDepthKnownId == Database.Models.Status.RecordStatusIds.Yes)
        {
            _isPeakDepthKnown = true;

            _peakDepthInsideMessage = CreatePeakDepthMessage(Entity.PeakInsideCentimetres);
            _peakDepthOutsideMessage = CreatePeakDepthMessage(Entity.PeakOutsideCentimetres);
        }
        else if(Entity.IsPeakDepthKnownId == Database.Models.Status.RecordStatusIds.No)
        {
            _peakDepthNotKnownMessage = "Not known";
        }
        else
        {
            _peakDepthNotKnownMessage = Unknown;
        }


        string CreatePeakDepthMessage(int? depthInCentimetres)
            => $@"{depthInCentimetres.ConvertMeasurementToDisplayString(StringExtensions.MeasurementDisplayType.MetresAndCentimetres)}
                 ({depthInCentimetres.ConvertMeasurementToDisplayString(StringExtensions.MeasurementDisplayType.FeetAndInches)})";
    }


    private void GetServiceImpact()
        => PopulateLabels(ShowServiceImpacts, ref _serviceImpactLabels, Entity.ServiceImpacts, s => s.FloodImpact.TypeName);


    private void GetCommunityImpact()
        => PopulateLabels(ShowCommunityImpacts, ref _communityImpactLabels, Entity.CommunityImpacts, c => c.FloodImpact.TypeName);


    private void GetBlockages()
        => PopulateLabel(ShowBlockages, ref _blockagesKnownProblemsLabel, b => b.KnownProblemDetails);


    private void GetActionsTaken()
        => PopulateLabels(ShowActionsTaken, ref _actionsTakenLabels, Entity.ActionsTaken, a => a.FloodMitigation.TypeName);


    private void GetHistory()
        => PopulateLabel(ShowHistory, ref _historyOfFloodingLabel, h => h.HistoryOfFlooding.Text);


    private void GetInsurance()
        => PopulateLabel(ShowInsurance, ref _propertyInsuredLabel, p => p.PropertyInsured.Text);


    private void GetHelpReceivedWarnings()
        => PopulateLabels(ShowHelpReceivedWarnings, ref _helpReceivedLabels, Entity.HelpReceived, h => h.FloodMitigation.TypeName);


    private void GetBeforeTheFloodingWarnings()
    {
        PopulateLabel(ShowBeforeFloodingWarnings, ref _registeredWithFloodlineLabel, r => r.Floodline.Text);
        PopulateLabel(ShowBeforeFloodingWarnings, ref _otherWarningReceivedLabel, o => o.WarningReceived.Text);
    }


    private void GetWarningSources()
        => PopulateLabels(ShowWarningSources, ref _warningSourcesLabels, Entity.WarningSources, w => w.FloodMitigation.TypeName);


    private void GetFloodlineWarnings()
    {
        _isFloodlineWarning = ShowFloodlineWarnings
            && Entity.FloodlineId == FloodOnlineReportingTool.Database.Models.Status.RecordStatusIds.Yes
            && Entity.WarningSources.Any(m => m.FloodMitigation.Id == FloodMitigationIds.FloodlineWarning);
        PopulateLabel(_isFloodlineWarning, ref _warningTimelyLabel, w => w.WarningTimely?.Text);
        PopulateLabel(_isFloodlineWarning, ref _warningAppropriateLabel, w => w.WarningAppropriate?.Text);
    }   


    private void PopulateLabel(bool showLabel, ref string? labelToPoulate, Func<Investigation, string?> generateLabelFunc)
    {
        if (!showLabel)
        {
            labelToPoulate = null;
            return;
        }

        labelToPoulate = generateLabelFunc(Entity);
    }

    private void PopulateLabels<T>(bool showLabel, ref string[] labelsToPoulate, IList<T> listFromWhichToPopulate, Func<T, string?> generateLabelsFunc)
    {
        if (!showLabel)
        {
            labelsToPoulate = [];
            return;
        }

        if (listFromWhichToPopulate.Count == 0)
        {
            labelsToPoulate = [Unknown];
            return;
        }

        labelsToPoulate = [.. listFromWhichToPopulate
            .Select(generateLabelsFunc)
            .Select(x => x ?? Unknown)];
    }


    public async ValueTask DisposeAsync()
    {
        try
        {
            await _cts.CancelAsync();
            _cts.Dispose();
        }
        finally
        {
            GC.SuppressFinalize(this);
        }
    }
}
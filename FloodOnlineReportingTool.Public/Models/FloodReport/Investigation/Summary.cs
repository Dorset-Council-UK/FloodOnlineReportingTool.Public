namespace FloodOnlineReportingTool.Public.Models.FloodReport.Investigation;

public class Summary
{
    // Water speed
    public string BeginLabel { get; set; } = "";
    public string WaterSpeedLabel { get; set; } = "";
    public string AppearanceLabel { get; set; } = "";

    // Water destination
    public IReadOnlyCollection<string> DestinationLabels { get; set; } = [];

    // Damaged vehicles
    public string VehiclesDamagedMessage { get; set; } = "";

    // Internal
    public bool IsInternal { get; set; }
    // Internal - How it entered - Water entry
    public IReadOnlyCollection<string> EntryLabels { get; set; } = [];
    // Internal - When it entered
    public string InternalMessage { get; set; } = "";

    // Peak depth
    public Guid IsPeakDepthKnownId { get; set; }
    public string PeakDepthMessage { get; set; } = "";
    public string PeakDepthInsideMessage { get; set; } = "";
    public string PeakDepthOutsideMessage { get; set; } = "";

    // Community impact
    public IReadOnlyCollection<string> CommunityImpactLabels { get; set; } = [];

    // Blockages
    public string HasKnownProblemsMessage { get; set; } = "";

    // Actions taken
    public IReadOnlyCollection<string> ActionsTakenLabels { get; set; } = [];

    // Help received
    public IReadOnlyCollection<string> HelpReceivedLabels { get; set; } = [];

    // Before the flooding - Warnings
    public string RegisteredWithFloodlineLabel { get; set; } = "";
    public string OtherWarningReceivedLabel { get; set; } = "";

    // Warning sources
    public IReadOnlyCollection<string> WarningSourceLabels { get; set; } = [];

    // Floodline warnings
    public bool IsFloodlineWarning { get; set; }
    public string WarningTimelyLabel { get; set; } = "";
    public string WarningAppropriateLabel { get; set; } = "";

    // History
    public string HistoryOfFloodingLabel { get; set; } = "";
}

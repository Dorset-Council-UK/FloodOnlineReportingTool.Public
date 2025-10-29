using FloodOnlineReportingTool.Database.Models.Flood;
using FloodOnlineReportingTool.Database.Models.Status;


namespace FloodOnlineReportingTool.Database.Models.Investigate;

/// <summary>
/// Investigations, for example grants.
/// </summary>
public record Investigation
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public DateTimeOffset CreatedUtc { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedUtc { get; init; }

    // Water speed, which are related flood problems
    public required Guid BeginId { get; init; }
    public FloodProblem Begin { get; init; } = default!;
    public required Guid WaterSpeedId { get; init; }
    public FloodProblem WaterSpeed { get; init; } = default!;
    public required Guid AppearanceId { get; init; }
    public FloodProblem Appearance { get; init; } = default!;
    public string? MoreAppearanceDetails { get; init; }

    // Water destination, which are related flood problems
    public IList<InvestigationDestination> Destinations { get; init; } = [];

    // Damaged vehicles, which is a related record status
    public required Guid WereVehiclesDamagedId { get; init; }
    public RecordStatus WereVehiclesDamaged { get; init; } = default!;
    public byte? NumberOfVehiclesDamaged { get; init; }

    // Internal (optional only needed when eligibility check is internal)
    // Internal - How it entered - Water entry, which are related flood problems
    public IList<InvestigationEntry> Entries { get; init; } = [];
    public string? WaterEnteredOther { get; init; }
    // Internal - When it entered, which is a related record status
    public Guid? WhenWaterEnteredKnownId { get; init; }
    public RecordStatus? WhenWaterEnteredKnown { get; init; }
    public DateTimeOffset? FloodInternalUtc { get; init; }

    // Peak depth, which is a related record status
    public required Guid IsPeakDepthKnownId { get; init; }
    public RecordStatus IsPeakDepthKnown { get; init; } = default!;
    public int? PeakInsideCentimetres { get; init; }
    public int? PeakOutsideCentimetres { get; init; }

    // Community impact, which are related flood impacts
    public IList<InvestigationCommunityImpact> CommunityImpacts { get; init; } = [];

    // Blockages
    public required bool HasKnownProblems { get; init; }
    public string? KnownProblemDetails { get; init; }

    // Actions taken, which ate related flood mitigations
    public IList<InvestigationActionsTaken> ActionsTaken { get; init; } = [];
    public string? OtherAction { get; init; }

    // Help received, which are related flood mitigations
    public IList<InvestigationHelpReceived> HelpReceived { get; init; } = [];

    // Before the flooding - Warnings, which are related record statuses
    public required Guid FloodlineId { get; init; }
    public RecordStatus Floodline { get; init; } = default!;
    public required Guid WarningReceivedId { get; init; }
    public RecordStatus WarningReceived { get; init; } = default!;

    // Warning sources, which are related flood mitigations
    public IList<InvestigationWarningSource> WarningSources { get; init; } = [];
    public string? WarningSourceOther { get; init; }

    // Floodline warnings (optional), which are related record statuses
    public Guid? WarningTimelyId { get; init; }
    public RecordStatus? WarningTimely { get; init; }
    public Guid? WarningAppropriateId { get; init; }
    public RecordStatus? WarningAppropriate { get; init; }

    // History, which is a related record status
    public required Guid HistoryOfFloodingId { get; init; }
    public RecordStatus HistoryOfFlooding { get; init; } = default!;
    public string? HistoryOfFloodingDetails { get; init; }
}

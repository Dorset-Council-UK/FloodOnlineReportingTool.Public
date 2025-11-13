namespace FloodOnlineReportingTool.Database.Models.Investigate;

public record InvestigationDto
{
    // Internal fields
    public Guid? WhenWaterEnteredKnownId { get; init; }
    public DateTimeOffset? FloodInternalUtc { get; init; }

    // Peak depth fields
    public Guid? IsPeakDepthKnownId { get; init; }
    public int? PeakInsideCentimetres { get; init; }
    public int? PeakOutsideCentimetres { get; init; }

    // History fields
    public Guid? HistoryOfFloodingId { get; init; }
    public string? HistoryOfFloodingDetails { get; init; }

    // Water speed fields, which are related flood problems
    public Guid? BeginId { get; init; }
    public Guid? WaterSpeedId { get; init; }
    public Guid? AppearanceId { get; init; }
    public string? MoreAppearanceDetails { get; init; }

    // Water entry fields, which are related flood problems
    public IList<Guid> Entries { get; init; } = [];
    public string? WaterEnteredOther { get; init; }

    // Water destination fields, which are related flood problems
    public IList<Guid> Destinations { get; init; } = [];

    // Vehicle fields
    public Guid? WereVehiclesDamagedId { get; init; }
    public byte? NumberOfVehiclesDamaged { get; init; }

    // Blockages fields
    public bool? HasKnownProblems { get; init; }
    public string? KnownProblemDetails { get; init; }

    // Community impact fields, which are related flood impacts
    public IList<Guid> CommunityImpacts { get; init; } = [];

    // Help received fields, which are related flood mitigations
    public IList<Guid> HelpReceived { get; init; } = [];

    // Actions taken fields, which are related flood mitigations
    public IList<Guid> ActionsTaken { get; init; } = [];
    public string? OtherAction { get; init; }

    // Warning fields
    public Guid? FloodlineId { get; init; }
    public Guid? WarningReceivedId { get; init; }

    // Warning source fields
    public IList<Guid> WarningSources { get; init; } = [];
    public string? WarningSourceOther { get; init; }

    // Floodline warnings fields
    public Guid? WarningTimelyId { get; init; }
    public Guid? WarningAppropriateId { get; init; }
}

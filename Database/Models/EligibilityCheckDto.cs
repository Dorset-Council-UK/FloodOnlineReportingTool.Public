namespace FloodOnlineReportingTool.Database.Models;

/// <summary>
/// A data transfer object representing an eligibility check. Only the data which can be changed.
/// </summary>
public record EligibilityCheckDto
{
    public long? Uprn { get; init; }
    public double Easting { get; init; }
    public double Northing { get; init; }
    public bool IsAddress { get; init; } = true;
    public string? LocationDesc { get; init; }
    public DateTimeOffset? ImpactStart { get; init; }
    public Guid? DurationKnownId { get; init; }
    public int? ImpactDuration { get; init; } // In hours
    public bool OnGoing { get; init; }
    public bool? Uninhabitable { get; init; }
    public Guid VulnerablePeopleId { get; init; }
    public int? VulnerableCount { get; init; }
    public IList<Guid> Residentials { get; init; } = [];
    public IList<Guid> Commercials { get; init; } = [];
    public IList<Guid> Sources { get; init; } = [];
}

using FloodOnlineReportingTool.Contracts;

namespace FloodOnlineReportingTool.Database.Models;

/// <summary>
/// A data transfer object representing an eligibility check data required for messages. Only the data which can be changed.
/// </summary>
public record EligibilityCheckMessageDto
{
    public Guid Id { get; init; }
    public DateTimeOffset CreatedUtc { get; init; }
    public DateTimeOffset? UpdatedUtc { get; init; }
    public long? Uprn { get; init; }
    public double Easting { get; init; }
    public double Northing { get; init; }
    public bool IsAddress { get; init; }
    public string? LocationDesc { get; init; }
    public DateTimeOffset? ImpactStart { get; init; }
    public int ImpactDuration { get; init; } // In hours
    public bool OnGoing { get; init; }
    public bool Uninhabitable { get; init; }
    public int? VulnerableCount { get; init; }
    public IReadOnlyCollection<EligibilityCheckOrganisation> Organisations { get; set; } = [];
}
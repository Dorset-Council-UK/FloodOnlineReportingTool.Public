using FloodOnlineReportingTool.Contracts;

namespace FloodOnlineReportingTool.Database.Models;

/// <summary>
/// Represents an assessment to determine if a person qualifies for assistance, related to flood damage.
/// </summary>
public record EligibilityCheck
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public DateTimeOffset CreatedUtc { get; init; }
    public DateTimeOffset? UpdatedUtc { get; init; }
    public long? Uprn { get; init; }
    public long? Usrn { get; init; }
    public double Easting { get; init; }
    public double Northing { get; init; }
    public bool IsAddress { get; init; }
    public string? LocationDesc { get; init; }
    public DateTimeOffset? ImpactStart { get; init; }
    public int ImpactDuration { get; init; } // In hours
    public bool OnGoing { get; init; }
    public bool Uninhabitable { get; init; }
    public Guid VulnerablePeopleId { get; init; }
    public RecordStatus? VulnerablePeople { get; init; }
    public int? VulnerableCount { get; init; }
    public DateTimeOffset TermsAgreed { get; init; }
    public IList<EligibilityCheckResidential> Residentials { get; init; } = [];
    public IList<EligibilityCheckCommercial> Commercials { get; init; } = [];
    public IList<EligibilityCheckSource> Sources { get; init; } = [];
    public IList<EligibilityCheckRunoffSource> SecondarySources { get; init; } = [];
}

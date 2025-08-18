namespace FloodOnlineReportingTool.Database.Models;

/// <summary>
/// An immutable record to hold the Address search or Nearest address search API data
/// </summary>
public record ApiAddress
{
    public long UPRN { get; init; }
    public long USRN { get; init; }
    public required string ConcatenatedAddress { get; init; }
    public string? Organisation { get; init; }
    public string? DepartmentName { get; init; }
    public string? Name { get; init; }
    public string? Number { get; init; }
    public string? Primary { get; init; }
    public string? Secondary { get; init; }
    public string? Street { get; init; }
    public string? Locality { get; init; }
    public string? TownVillage { get; init; }
    public string? PostTown { get; init; }
    public string? LocalAuthorityName { get; init; }
    public required string Postcode { get; init; }
    public string? PrimaryClassification { get; init; }
    public string? SecondaryClassification { get; init; }
    public string? TertiaryClassification { get; init; }
    public string? QuaternaryClassification { get; init; }
    public string? Status { get; init; }
    public double Easting { get; init; }
    public double Northing { get; init; }
    public double Latitude { get; init; }
    public double Longitude { get; init; }
}

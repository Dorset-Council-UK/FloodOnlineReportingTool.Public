namespace FloodOnlineReportingTool.Public.Models.FloodReport.Update;

public class UpdateModel
{
    public required Guid Id { get; init; }
    public required DateTimeOffset CreatedUtc { get; init; }
    public required DateTimeOffset? UpdatedUtc { get; init; }
    public long Uprn { get; set; }
    public double Easting { get; set; }
    public double Northing { get; set; }
    public string? LocationDesc { get; set; }

    // TODO: Decide which fields we want to be view only or updatable on the eligibility check
}

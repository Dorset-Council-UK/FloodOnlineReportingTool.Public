namespace FloodOnlineReportingTool.Public.Models.FloodReport.Update;

public class UpdateModel
{
    public required Guid Id { get; init; }
    public required DateTimeOffset CreatedUtc { get; init; }
    public required DateTimeOffset? UpdatedUtc { get; init; }
    public string? UprnText { get; set; }
    public int? UprnNumber { get; set; }
    public string? EastingText { get; set; }
    public float? EastingNumber { get; set; }
    public string? NorthingText { get; set; }
    public float? NorthingNumber { get; set; }
    public string? LocationDesc { get; set; }

    // TODO: Decide which fields we want to be view only or updatable on the eligibility check
}

namespace FloodOnlineReportingTool.Public.Models.FloodReport.Create;

public class Summary
{
    public IReadOnlyCollection<string> FloodedAreas { get; set; } = [];

    public IReadOnlyCollection<string> FloodSources { get; set; } = [];
    public bool IsAddress {  get; set; }

    public string? AddressPreview { get; set; }

    public string? PropertyTypeName { get; set; }

    public bool? IsUninhabitable { get; set; }

    public DateTimeOffset? StartDate { get; set; }

    public bool? IsOnGoing { get; set; }

    public int? FloodDurationHours { get; set; }

    public Guid? VulnerablePeopleId { get; set; }

    public int? NumberOfVulnerablePeople { get; set; }
}

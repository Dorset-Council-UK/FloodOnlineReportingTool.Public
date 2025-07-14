namespace FloodOnlineReportingTool.Public.Models.FloodReport.Create;

public record ExtraData
{
    public string? Postcode { get; init; }

    public string? PrimaryClassification { get; init; }

    public string? SecondaryClassification { get; init; }

    public Guid? PropertyType { get; init; }
    public IList<MediaItem>? Media { get; set; }
}
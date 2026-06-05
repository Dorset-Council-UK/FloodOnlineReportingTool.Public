namespace FloodOnlineReportingTool.Public.Models.FloodReport.Create;

public class MediaItem
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
}

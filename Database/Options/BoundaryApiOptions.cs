namespace FloodOnlineReportingTool.Database.Options;

public class BoundaryApiOptions
{
    public const string ServiceName = "BoundaryApi";
    public const string SectionName = $"DownstreamAPIs:{ServiceName}";

    public string BaseUrl { get; set; } = "";
    public string ApiKey { get; set; } = "";
    public string ApiKeyHeaderName { get; set; } = "Ocp-Apim-Subscription-Key";
    public string GetCountyPathTemplate { get; set; } = "";
}
namespace FloodOnlineReportingTool.Public.Options;

public class GovNotifyOptions
{
    public const string SectionName = "GovNotify";

    public required string ApiKey { get; init; }
    public required GovNotifyTemplates Templates { get; init; }
}

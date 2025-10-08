namespace FloodOnlineReportingTool.Public.Settings;

public class GovNotifySettings
{
    public const string SectionName = "GovNotify";

    public required string ApiKey { get; init; }
    public required GovNotifyTemplates Templates { get; init; }
}

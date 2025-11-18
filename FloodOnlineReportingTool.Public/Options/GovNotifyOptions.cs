using FloodOnlineReportingTool.Database.Options;

namespace FloodOnlineReportingTool.Public.Options;

public class GovNotifyOptions : IConfigSection
{
    public static string SectionName => "GovNotify";

    public required string ApiKey { get; init; }
    public required GovNotifyTemplates Templates { get; init; }
}

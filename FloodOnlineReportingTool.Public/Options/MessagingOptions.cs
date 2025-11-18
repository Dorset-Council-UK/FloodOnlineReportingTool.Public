using FloodOnlineReportingTool.Database.Options;

namespace FloodOnlineReportingTool.Public.Options;

public class MessagingOptions : IConfigSection
{
    public static string SectionName => "Messaging";

    public required bool Enabled { get; init; } = false;
    public required string ConnectionString { get; init; }
}

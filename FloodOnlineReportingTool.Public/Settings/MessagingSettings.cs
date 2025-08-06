namespace FloodOnlineReportingTool.Public.Settings;

public class MessagingSettings
{
    public const string SectionName = "Messaging";

    public required bool Enabled { get; init; } = false;
    public required string ConnectionString { get; init; }
}

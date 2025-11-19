namespace FloodOnlineReportingTool.Public.Options;

public class MessagingOptions 
{
    public const string SectionName = "Messaging";

    public required bool Enabled { get; init; } = false;
    public required string ConnectionString { get; init; }
}

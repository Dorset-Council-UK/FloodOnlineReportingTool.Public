namespace FloodOnlineReportingTool.Public.Settings;

internal class RabbitMqSettings
{
    public const string SectionName = "RabbitMQ";

    public required bool Enabled { get; init; }
    public required Uri Host { get; init; }
    public required Uri HostContainer { get; init; }
    public required string Username { get; init; }
    public required string Password { get; init; }
}

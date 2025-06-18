namespace FloodOnlineReportingTool.Public.Models.OpenApi;

internal class Versions
{
    internal static VersionInfo V1 = new(new(1, 0));
    internal static IReadOnlyCollection<VersionInfo> All = [V1];
}
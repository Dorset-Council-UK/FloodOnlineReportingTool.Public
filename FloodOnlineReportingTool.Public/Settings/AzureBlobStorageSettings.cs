namespace FloodOnlineReportingTool.Public.Settings;

internal class AzureBlobStorageSettings
{
    public const string SectionName = "AzureBlobStorage";

    public string ConnectionString { get; init; } = "";
    public string ContainerName { get; init; } = "";
}

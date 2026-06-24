namespace FloodOnlineReportingTool.Public.Options;

public class AzureBlobStorageOptions
{
    public const string SectionName = "AzureBlobStorage";
    public required string ConnectionString { get; init; }
    public required string ContainerName { get; init; }
    public string ReadSASToken { get; init; } = "";
} 
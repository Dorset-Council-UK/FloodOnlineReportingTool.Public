namespace FloodOnlineReportingTool.Public.Options;

public record KeyVaultAzureAdOptions
{
    public const string SectionName = "AzureAd";

    public string ApplicationId { get; init; } = "";
    public string CertificateThumbprint { get; init; } = "";
    public string DirectoryId { get; init; } = "";
}

namespace FloodOnlineReportingTool.Public.Settings;

internal class KeyVaultSettings
{
    public const string SectionName = "KeyVault";

    public required string Name { get; init; }
    public required KeyVaultAzureAdSettings AzureAd { get; init; }
}
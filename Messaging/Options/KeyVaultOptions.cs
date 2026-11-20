namespace Messaging.Options;

public record KeyVaultOptions
{
    public const string SectionName = "KeyVault";

    public string VaultName { get; init; } = "";
    public string SharedVaultName { get; init; } = "";
}

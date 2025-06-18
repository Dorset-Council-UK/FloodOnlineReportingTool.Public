namespace FloodOnlineReportingTool.Public.Settings;

internal class KeyVaultAzureAdSettings
{
    public const string SectionName = "AzureAd";
    /// <summary>
    /// The thumbprint of the certificate used to authenticate to Azure AD.
    /// </summary>
    public required string CertificateThumbprint { get; init; } = "";
    /// <summary>
    /// The directory id of the Azure AD instance.
    /// </summary>
    public required string DirectoryId { get; init; } = "";
    /// <summary>
    /// The application id of the Azure AD application.
    /// </summary>
    public required string ApplicationId { get; init; } = "";
}

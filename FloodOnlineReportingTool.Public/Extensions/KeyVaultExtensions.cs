using Azure.Identity;
using FloodOnlineReportingTool.Public.Settings;
using System.Security.Cryptography.X509Certificates;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.AspNetCore.Builder;
#pragma warning restore IDE0130 // Namespace does not match folder structure

internal static class KeyVaultExtensions
{
    internal static IConfigurationManager AddFloodReportingKeyVault(this IConfigurationManager configuration)
    {
        // if keyvault options exist then we use keyvault, otherwise we ignore and use whatever local settings (appSettings, user secrets etc.) are used
        var keyVaultSettings = GetKeyVaultSettings(configuration);
        if (keyVaultSettings == null)
        {
            return configuration;
        }

        using var x509Store = new X509Store(StoreLocation.LocalMachine);
        x509Store.Open(OpenFlags.ReadOnly);

        var x509Certificate = x509Store.Certificates
            .Find(X509FindType.FindByThumbprint, keyVaultSettings.AzureAd.CertificateThumbprint, validOnly: false)
            .OfType<X509Certificate2>()
            .Single();

        configuration.AddAzureKeyVault(
            new Uri($"https://{keyVaultSettings.Name}.vault.azure.net/"),
            new ClientCertificateCredential(keyVaultSettings.AzureAd.DirectoryId, keyVaultSettings.AzureAd.ApplicationId, x509Certificate));

        return configuration;
    }

    private static KeyVaultSettings? GetKeyVaultSettings(IConfigurationManager configuration)
    {
        var keyVaultSection = configuration.GetSection(KeyVaultSettings.SectionName);
        var keyVaultAzureAdSection = keyVaultSection.GetSection(KeyVaultAzureAdSettings.SectionName);

        if (!keyVaultSection.Exists() || !keyVaultAzureAdSection.Exists())
        {
            return null;
        }

        var settings = keyVaultSection.Get<KeyVaultSettings>();
        if (settings == null || string.IsNullOrWhiteSpace(settings.Name) || string.IsNullOrWhiteSpace(settings.AzureAd.CertificateThumbprint) ||
            string.IsNullOrWhiteSpace(settings.AzureAd.DirectoryId) || string.IsNullOrWhiteSpace(settings.AzureAd.ApplicationId))
        {
            return null;
        }

        return settings;
    }
}

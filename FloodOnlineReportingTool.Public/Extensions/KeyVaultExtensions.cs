using Azure.Identity;
using FloodOnlineReportingTool.Public.Settings;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.AspNetCore.Builder;

internal static class KeyVaultExtensions
{
    internal static IConfigurationManager AddFloodReportingKeyVault(this IConfigurationManager configuration, KeyVaultSettings keyVaultSettings)
    {
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
}

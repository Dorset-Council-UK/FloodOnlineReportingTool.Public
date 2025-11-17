using Azure.Identity;
using FloodOnlineReportingTool.Public.Options;
using Microsoft.Extensions.Options;
using System.Security.Cryptography.X509Certificates;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.AspNetCore.Builder;
#pragma warning restore IDE0130 // Namespace does not match folder structure

internal static class KeyVaultExtensions
{
    internal static TBuilder AddKeyVaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        // if keyvault options exist then we use keyvault, otherwise we ignore and use whatever local settings (appSettings, user secrets etc.) are used
        var keyvaultSection = builder.Configuration
            .GetRequiredSection(KeyVaultOptions.SectionName).Get<KeyVaultOptions>();
        var azureAdSection = builder.Configuration
            .GetRequiredSection(AzureAdOptions.SectionName).Get<AzureAdOptions>();

        if (keyvaultSection == null || azureAdSection == null || string.IsNullOrEmpty(keyvaultSection.VaultName))
        {
            return builder;
        }

        using var x509Store = new X509Store(StoreLocation.LocalMachine);
        x509Store.Open(OpenFlags.ReadOnly);

        var x509Certificate = x509Store.Certificates
            .Find(X509FindType.FindByThumbprint, azureAdSection.ClientCertificateThumbprint, validOnly: false)
            .OfType<X509Certificate2>()
            .Single();

        builder.Configuration.AddAzureKeyVault(
            new Uri($"https://{keyvaultSection.VaultName}.vault.azure.net/"),
            new ClientCertificateCredential(azureAdSection.TenantId, azureAdSection.ClientId, x509Certificate));

        if (string.IsNullOrEmpty(keyvaultSection.SharedVaultName))
        {
            return builder;
        }

        builder.Configuration.AddAzureKeyVault(
            new Uri($"https://{keyvaultSection.SharedVaultName}.vault.azure.net/"),
            new ClientCertificateCredential(azureAdSection.TenantId, azureAdSection.ClientId, x509Certificate));

        return builder;
    }
}

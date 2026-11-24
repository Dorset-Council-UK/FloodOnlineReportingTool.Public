using Azure.Identity;
using Messaging.Options;

namespace Microsoft.Extensions.Hosting;

internal static class KeyVaultExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        internal IHostApplicationBuilder AddKeyVaults()
        {
            var keyVaultOptions = builder.Configuration
                .GetSection(KeyVaultOptions.SectionName)
                .Get<KeyVaultOptions>();

            if (keyVaultOptions is null)
            {
                return builder;
            }

            // This KeyVault has options shared over multiple projects
            if (!string.IsNullOrWhiteSpace(keyVaultOptions.SharedVaultName))
            {
                builder.Configuration.AddAzureKeyVault(
                    new Uri($"https://{keyVaultOptions.SharedVaultName}.vault.azure.net/"),
                    new DefaultAzureCredential());
            }

            // This KeyVault is specific to this project
            if (!string.IsNullOrWhiteSpace(keyVaultOptions.VaultName))
            {
                builder.Configuration.AddAzureKeyVault(
                    new Uri($"https://{keyVaultOptions.VaultName}.vault.azure.net/"),
                    new DefaultAzureCredential());
            }

            return builder;
        }
    }
}

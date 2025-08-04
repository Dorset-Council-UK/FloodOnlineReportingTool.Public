using FloodOnlineReportingTool.DataAccess.Exceptions;
using FloodOnlineReportingTool.DataAccess.Settings;
using FloodOnlineReportingTool.Public.Settings;

namespace Microsoft.AspNetCore.Builder;

internal static class SettingsExtensions
{
    internal static (KeyVaultSettings?, MessagingSettings, GISSettings) AddFloodReportingSettings(this IServiceCollection services, IConfigurationManager configuration)
    {
        // Sections
        var gisSection = configuration.GetSection(GISSettings.SectionName)
            ?? throw new ConfigurationMissingException($"Configuration section '{GISSettings.SectionName}' is missing.");
        var keyVaultSection = gisSection.GetSection(KeyVaultSettings.SectionName);
        var messagingSection = configuration.GetSection(MessagingSettings.SectionName)
            ?? throw new ConfigurationMissingException($"Configuration section '{MessagingSettings.SectionName}' is missing.");

        // Configure settings
        services.Configure<GISSettings>(gisSection);
        services.Configure<MessagingSettings>(messagingSection);
        var hasKeyVaultSettings = keyVaultSection.Value != null;
        if (hasKeyVaultSettings)
        {
            services.Configure<KeyVaultSettings>(keyVaultSection);
        }

        // Settings
        var keyVaultSettings = hasKeyVaultSettings ? keyVaultSection.Get<KeyVaultSettings>() : null;
        var messagingSettings = messagingSection.Get<MessagingSettings>()
            ?? throw new ConfigurationMissingException($"Configuration section '{MessagingSettings.SectionName}' is not properly defined.");
        var gisSettings = gisSection.Get<GISSettings>()
            ?? throw new ConfigurationMissingException($"Configuration section '{GISSettings.SectionName}' is not properly defined.");

        return (keyVaultSettings, messagingSettings, gisSettings);
    }
}

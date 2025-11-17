using FloodOnlineReportingTool.Database.Exceptions;
using FloodOnlineReportingTool.Database.Settings;
using FloodOnlineReportingTool.Public.Options;
using Microsoft.Identity.Web;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.AspNetCore.Builder;
#pragma warning restore IDE0130 // Namespace does not match folder structure

internal static class SettingsExtensions
{
    internal static (MessagingOptions, GISSettings, MicrosoftIdentityOptions?) AddFloodReportingSettings(this IServiceCollection services, IConfigurationManager configuration)
    {
        // Sections
        var gisSection = configuration.GetSection(GISSettings.SectionName)
            ?? throw new ConfigurationMissingException($"Configuration section '{GISSettings.SectionName}' is missing.");
        var messagingSection = configuration.GetRequiredSection(MessagingOptions.SectionName)
            ?? throw new ConfigurationMissingException($"Configuration section '{MessagingOptions.SectionName}' is missing.");
        var identitySection = configuration.GetSection(Constants.AzureAd);

        // Configure settings
        services.Configure<GISSettings>(gisSection);
        services.Configure<MessagingOptions>(messagingSection);

        // Settings
        var messagingSettings = messagingSection.Get<MessagingOptions>()
            ?? throw new ConfigurationMissingException($"Configuration section '{MessagingOptions.SectionName}' is not properly defined.");
        var gisSettings = gisSection.Get<GISSettings>()
            ?? throw new ConfigurationMissingException($"Configuration section '{GISSettings.SectionName}' is not properly defined.");
        MicrosoftIdentityOptions? identityOptions = identitySection?.Get<MicrosoftIdentityOptions>();

        return (messagingSettings, gisSettings, identityOptions);
    }
}

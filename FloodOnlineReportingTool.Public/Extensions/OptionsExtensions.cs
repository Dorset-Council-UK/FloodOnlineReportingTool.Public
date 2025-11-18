using FloodOnlineReportingTool.Database.Options;
using FloodOnlineReportingTool.Public.Options;
using Microsoft.Identity.Web;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.AspNetCore.Builder;
#pragma warning restore IDE0130 // Namespace does not match folder structure

internal static class OptionsExtensions
{
    internal static (MessagingOptions, GISOptions, MicrosoftIdentityOptions?) AddFloodReportingSettings<TBuilder>(this TBuilder builder) 
        where TBuilder : IHostApplicationBuilder
    {
        var messagingSettings = AddRequired<MessagingOptions, TBuilder>(builder, MessagingOptions.SectionName);
        var gisSettings = AddRequired<GISOptions, TBuilder>(builder, GISOptions.SectionName);
        var identityOptions = AddOptional<MicrosoftIdentityOptions, TBuilder>(builder, "AzureAd");

        return (messagingSettings, gisSettings, identityOptions);
    }

    private static T? AddOptional<T, TBuilder>(
        TBuilder builder, 
        string sectionName
    ) 
        where T : class 
        where TBuilder : IHostApplicationBuilder
    {
        var section = builder.Configuration.GetSection(sectionName);
        builder.Services.Configure<T>(section);
        return section.Get<T>();
    }

    private static T AddRequired<T, TBuilder>(
        TBuilder builder,
        string sectionName
    )
        where T : class, IConfigSection
        where TBuilder : IHostApplicationBuilder
    {
        var section = builder.Configuration.GetSection(sectionName);
        builder.Services.Configure<T>(section);
        var options = section.Get<T>()
            ?? throw new InvalidOperationException($"Configuration section '{T.SectionName}' is not properly defined.");
        return options;
    }
}

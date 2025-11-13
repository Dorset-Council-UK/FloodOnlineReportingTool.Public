using FloodOnlineReportingTool.Public.Services;
using FloodOnlineReportingTool.Public.Settings;
using Notify.Client;
using Notify.Interfaces;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.AspNetCore.Builder;
#pragma warning restore IDE0130 // Namespace does not match folder structure

internal static class GovNotifyExtensions
{
    private const string GovNotifyClientName = "GovNotifyClient";

    public static TBuilder AddGovNotify<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        var govNotifySettings = builder.AddGovNotifySettings();
        builder.Services
            .AddGovNotifyHttpClient()
            .AddGovNotifyHttpClientWrapper()
            .AddGovNotifyNotificationClient(govNotifySettings.ApiKey)
            .AddScoped<IGovNotifyEmailSender, GovNotifyEmailSender>();

        return builder;
    }

    private static GovNotifySettings AddGovNotifySettings<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        var section = builder.Configuration.GetRequiredSection(GovNotifySettings.SectionName);
        builder.Services.Configure<GovNotifySettings>(section);
        var settings = section.Get<GovNotifySettings>();
        return settings is null
            ? throw new InvalidOperationException($"There was a problem getting the settings from the {GovNotifySettings.SectionName} section")
            : settings;
    }

    /// <summary>
    /// Setup the GovNotify HttpClient using the HttpClientFactory
    /// </summary>
    /// <remarks>Uses Microsoft's new Http resilience library <see href="https://www.nuget.org/packages/Microsoft.Extensions.Http.Resilience/">Microsoft.Extensions.Http.Resilience</see></remarks>
    private static IServiceCollection AddGovNotifyHttpClient(this IServiceCollection services)
    {
        services
            .AddHttpClient(GovNotifyClientName)
            .AddStandardResilienceHandler();

        return services;
    }

    /// <summary>
    /// Setup the GovNotify HttpClientWrapper using the HttpClientFactory
    /// </summary>
    private static IServiceCollection AddGovNotifyHttpClientWrapper(this IServiceCollection services)
    {
        services.AddTransient<IHttpClient>(serviceProvider =>
        {
            var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(GovNotifyClientName);
            return new HttpClientWrapper(httpClient);
        });

        return services;
    }

    /// <summary>
    /// Setup the GovNotify NotificationClient using the configured HttpClientWrapper
    /// </summary>
    private static IServiceCollection AddGovNotifyNotificationClient(this IServiceCollection services, string apiKey)
    {
        services.AddTransient<NotificationClient>(serviceProvider =>
        {
            var httpClientWrapper = serviceProvider.GetRequiredService<IHttpClient>();
            return new NotificationClient(httpClientWrapper, apiKey);
        });

        return services;
    }
}

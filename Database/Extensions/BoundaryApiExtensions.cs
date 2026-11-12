using FloodOnlineReportingTool.Database.Options;
using FloodOnlineReportingTool.Database.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

public static class BoundaryApiExtensions
{
    public static IServiceCollection AddBoundaryApi(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetRequiredSection(BoundaryApiOptions.SectionName);
        services.AddOptions<BoundaryApiOptions>()
            .Bind(section)
            .Validate(options => Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var uri)
                && string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.Ordinal),
                $"{BoundaryApiOptions.SectionName}:BaseUrl must be an absolute HTTPS URL.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.ApiKey)
                && !options.ApiKey.Contains('\r') && !options.ApiKey.Contains('\n'),
                $"{BoundaryApiOptions.SectionName}:ApiKey is required and must be a valid header value.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.ApiKeyHeaderName)
                && options.ApiKeyHeaderName.All(c => char.IsAsciiLetterOrDigit(c) || "!#$%&'*+-.^_`|~".Contains(c)),
                $"{BoundaryApiOptions.SectionName}:ApiKeyHeaderName must be a valid HTTP header name.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.GetCountyPathTemplate)
                && options.GetCountyPathTemplate.Contains("{x}", StringComparison.Ordinal)
                && options.GetCountyPathTemplate.Contains("{y}", StringComparison.Ordinal)
                && Uri.IsWellFormedUriString(options.GetCountyPathTemplate.Replace("{x}", "0", StringComparison.Ordinal)
                    .Replace("{y}", "0", StringComparison.Ordinal), UriKind.Relative)
                && !options.GetCountyPathTemplate.StartsWith("/", StringComparison.Ordinal),
                $"{BoundaryApiOptions.SectionName}:GetCountyPathTemplate must be a relative path containing {{x}} and {{y}}.")
            .ValidateOnStart();

        services.AddHttpClient<IBoundaryApiClient, BoundaryApiClient>((provider, client) =>
            {
                var options = provider.GetRequiredService<IOptions<BoundaryApiOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/", UriKind.Absolute);
                client.DefaultRequestHeaders.Add(options.ApiKeyHeaderName, options.ApiKey);
            })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false });

        return services;
    }
}
using FloodOnlineReportingTool.Public.Models.OpenApi;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.AspNetCore.Builder;
#pragma warning restore IDE0130 // Namespace does not match folder structure

internal static class VersioningExtensions
{
    /// <summary>
    /// Add API versioning
    /// </summary>
    /// <remarks>
    ///     <para>Asp.Versioning.Http only includes support for Minimal APIs</para>
    ///     <para>See: <see href="https://www.nuget.org/packages/Asp.Versioning.Http/">Asp.Versioning.Mvc - nuget</see></para>
    ///     <para>See: <see href="https://github.com/dotnet/aspnet-api-versioning/wiki/New-Services-Quick-Start#aspnet-core-with-minimal-apis">Quick start with minimal APIs - github</see></para>
    /// </remarks>
    internal static IServiceCollection AddFloodReportingVersioning(this IServiceCollection services)
    {
        // Add API versioning
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = Versions.V1.Version;
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
        });

        return services;
    }
}

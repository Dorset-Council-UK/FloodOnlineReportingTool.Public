#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.AspNetCore.Builder;
#pragma warning restore IDE0130 // Namespace does not match folder structure

internal static class OptionsExtensions
{
    public static T? AddOptions_Optional<T>(
        this IHostApplicationBuilder builder, 
        string sectionName
    ) 
        where T : class 
    {
        var section = builder.Configuration.GetSection(sectionName);
        builder.Services.Configure<T>(section);
        return section.Get<T>();
    }

    public static T AddOptions_Required<T>(
        this IHostApplicationBuilder builder,
        string sectionName
    )
        where T : class
    {
        var section = builder.Configuration.GetRequiredSection(sectionName);
        builder.Services.Configure<T>(section);
        var options = section.Get<T>()
            ?? throw new InvalidOperationException($"Configuration section '{sectionName}' is not properly defined.");
        return options;
    }
}

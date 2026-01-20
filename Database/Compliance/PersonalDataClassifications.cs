using Microsoft.Extensions.Compliance.Classification;

namespace FloodOnlineReportingTool.Database.Compliance;

// Define the taxonomy
public static class PersonalDataClassifications
{
    public static string Name => "CustomTaxonomy";
    public static DataClassification Pii { get; } = new DataClassification(Name, nameof(Pii));
    public static DataClassification PiiRedaction { get; } = new DataClassification(Name, nameof(PiiRedaction));
}

#pragma warning disable MA0048
// Create a custom attribute for logging/telemetry redaction
//[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class PiiAttribute : DataClassificationAttribute
{
    public PiiAttribute()
        : base(PersonalDataClassifications.Pii)
    {
    }
}

//[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class PiiRedactionAttribute : DataClassificationAttribute
{
    public PiiRedactionAttribute()
        : base(PersonalDataClassifications.PiiRedaction)
    {
    }
}
#pragma warning restore MA0048
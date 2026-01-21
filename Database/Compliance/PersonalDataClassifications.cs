using Microsoft.Extensions.Compliance.Classification;

namespace FloodOnlineReportingTool.Database.Compliance;

// Define the taxonomy
public static class PersonalDataClassifications
{
    public static string Name => "CustomTaxonomy";
    public static DataClassification Pii { get; } = new DataClassification(Name, nameof(Pii));
}

#pragma warning disable MA0048
// Create a custom attribute for logging/telemetry redaction
public sealed class PiiAttribute : DataClassificationAttribute
{
    public PiiAttribute()
        : base(PersonalDataClassifications.Pii)
    {
    }
}
#pragma warning restore MA0048
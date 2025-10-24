using System.Collections.Immutable;

namespace FloodOnlineReportingTool.Database.Models.Flood.FloodProblemIds;

public static class FloodOnsetIds
{
    // Onset Id's
    public readonly static Guid Suddenly = new("018fe130-7380-7859-b90c-b5b6b347f75c");
    public readonly static Guid Gradually = new("018fe131-5de0-7929-b4f4-dc78a73a4af6");

    public readonly static ImmutableHashSet<Guid> All = [Suddenly, Gradually];
}

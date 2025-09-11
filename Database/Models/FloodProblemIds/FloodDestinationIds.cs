using System.Collections.Immutable;

namespace FloodOnlineReportingTool.Database.Models.FloodProblemIds;

public static class FloodDestinationIds
{
    // Destination Id's
    public readonly static Guid River = new("018fe20c-2d80-7e13-add2-2c72f510141d");
    public readonly static Guid StreamOrWatercourse = new("018fe20d-17e0-72fd-a679-ecdfcce68d1a");
    public readonly static Guid TheSea = new("018fe20e-0240-7a6b-ad35-1e4fc9bcd1c4");
    public readonly static Guid DitchesAndDrainageChannels = new("018fe20e-eca0-744b-95bf-b3a85b0e748b");
    public readonly static Guid RoadDrainage = new("018fe20f-d700-7097-bdf0-5d88714b5528");
    public readonly static Guid NotSure = new("018fe210-c160-7359-97f0-fdd430ed229c");

    public readonly static ImmutableHashSet<Guid> All = [
        River,
        StreamOrWatercourse,
        TheSea,
        DitchesAndDrainageChannels,
        RoadDrainage,
        NotSure,
    ];
}

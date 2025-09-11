using System.Collections.Immutable;

namespace FloodOnlineReportingTool.Database.Models.FloodProblemIds;

public static class FloodDurationIds
{
    // Duration Id's
    public readonly static Guid Duration1 = new("018fe19e-5080-7a35-a80f-ab36da09bba2");
    public readonly static Guid Duration24 = new("018fe19f-3ae0-783b-bebb-27c43b4d2df6");
    public readonly static Guid Duration168 = new("018fe1a0-2540-72a1-8204-316d1500d8bb");
    public readonly static Guid Duration744 = new("018fe1a1-0fa0-73d6-aa9b-495404a336f1");
    public readonly static Guid DurationKnown = new("018fe1a1-fa00-76e6-929f-c87588a3853b");
    public readonly static Guid DurationNotSure = new("018fe1a2-e460-76f0-b29d-663d0a19ddd8");

    public readonly static ImmutableHashSet<Guid> All = [
        Duration1,
        Duration24,
        Duration168,
        Duration744,
        DurationKnown,
        DurationNotSure,
    ];
}

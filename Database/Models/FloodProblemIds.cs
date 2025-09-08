using FloodOnlineReportingTool.Contracts.Shared;

namespace FloodOnlineReportingTool.Database.Models;

/// <summary>
/// The flood problem Id's.
/// Helps ensure consistency and allows easier comparison.
/// </summary>
public static class FloodProblemIds
{
    // Primary cause Id's
    public readonly static Guid PrimaryCauseRiver = FloodSourceIds.PrimaryCauseRiver;
    public readonly static Guid PrimaryCauseStreamOrWatercourse = FloodSourceIds.PrimaryCauseStreamOrWatercourse;
    public readonly static Guid LakeOrReservoir = FloodSourceIds.LakeOrReservoir;
    public readonly static Guid PrimaryCauseTheSea = FloodSourceIds.PrimaryCauseTheSea;
    public readonly static Guid PrimaryCauseDitchesAndDrainageChannels = FloodSourceIds.PrimaryCauseDitchesAndDrainageChannels;
    public readonly static Guid WaterRisingOutOfTheGround = FloodSourceIds.WaterRisingOutOfTheGround;
    public readonly static Guid FoulDrainageSewerage = FloodSourceIds.FoulDrainageSewerage;
    public readonly static Guid SurfaceWaterDrainage = FloodSourceIds.SurfaceWaterDrainage;
    public readonly static Guid BlockedRoadDrainage = FloodSourceIds.BlockedRoadDrainage;
    public readonly static Guid BridgeOrCulvert = FloodSourceIds.BridgeOrCulvert;
    public readonly static Guid WavesCausedByVehicles = FloodSourceIds.WavesCausedByVehicles;
    public readonly static Guid RainwaterFlowingOverTheGround = FloodSourceIds.RainwaterFlowingOverTheGround;
    public readonly static Guid PrimaryCauseNotSure = FloodSourceIds.PrimaryCauseNotSure;

    // Secondary cause Id's
    public readonly static Guid RunoffFromRoad = FloodSourceIds.RunoffFromRoad;
    public readonly static Guid RunoffFromPrivateRoad = FloodSourceIds.RunoffFromPrivateRoad;
    public readonly static Guid RunoffFromTrackOrPath = FloodSourceIds.RunoffFromTrackOrPath;
    public readonly static Guid RunoffFromAgriculturalLand = FloodSourceIds.RunoffFromAgriculturalLand;
    public readonly static Guid RunoffFromOtherProperty = FloodSourceIds.RunoffFromOtherProperty;
    public readonly static Guid SecondaryCauseNotSure = FloodSourceIds.SecondaryCauseNotSure;

    // Appearance Id's
    public readonly static Guid Clear = new("018fe0f9-8500-7fd2-8c09-55bcabc21fe8");
    public readonly static Guid Muddy = new("018fe0fa-6f60-7062-a8b6-00a898cc5648");
    public readonly static Guid PollutedWithSewage = new("018fe0fb-59c0-7bd1-9634-46897151ff78");

    // Onset Id's
    public readonly static Guid Suddenly = new("018fe130-7380-7859-b90c-b5b6b347f75c");
    public readonly static Guid Gradually = new("018fe131-5de0-7929-b4f4-dc78a73a4af6");

    // Speed Id's
    public readonly static Guid Fast = new("018fe167-6200-7e14-b35b-31be0af65be2");
    public readonly static Guid Slow = new("018fe168-4c60-7ba1-90ad-c75fc66ee698");
    public readonly static Guid Still = new("018fe169-36c0-7e6d-ac4d-83265c6a3fa6");

    // Duration Id's
    public readonly static Guid Duration1 = new("018fe19e-5080-7a35-a80f-ab36da09bba2");
    public readonly static Guid Duration24 = new("018fe19f-3ae0-783b-bebb-27c43b4d2df6");
    public readonly static Guid Duration168 = new("018fe1a0-2540-72a1-8204-316d1500d8bb");
    public readonly static Guid Duration744 = new("018fe1a1-0fa0-73d6-aa9b-495404a336f1");
    public readonly static Guid DurationKnown = new("018fe1a1-fa00-76e6-929f-c87588a3853b");
    public readonly static Guid DurationNotSure = new("018fe1a2-e460-76f0-b29d-663d0a19ddd8");

    // Entry Id's
    public readonly static Guid Door = new("018fe1d5-3f00-7ace-aa24-f7337da28d46");
    public readonly static Guid Windows = new("018fe1d6-2960-7166-b8bf-729fad31d8dc");
    public readonly static Guid Airbrick = new("018fe1d7-13c0-7730-8d86-d67c01ec14e5");
    public readonly static Guid Walls = new("018fe1d7-fe20-71c0-b136-778d46ed6717");
    public readonly static Guid ThroughFloor = new("018fe1d8-e880-7f2e-9b5e-876bd2770f4a");
    public readonly static Guid ExternalOnly = new("018fe1d9-d2e0-763d-9459-6aa643c1470b");
    public readonly static Guid EntryOther = new("018fe1da-bd40-7c13-842c-901f01c9158f");
    public readonly static Guid EntryNotSure = new("018fe1db-a7a0-7c33-a15d-a4b6fc1ad57e");

    // Destination Id's
    public readonly static Guid DestinationRiver = new("018fe20c-2d80-7e13-add2-2c72f510141d");
    public readonly static Guid DestinationStreamOrWatercourse = new("018fe20d-17e0-72fd-a679-ecdfcce68d1a");
    public readonly static Guid DestinationTheSea = new("018fe20e-0240-7a6b-ad35-1e4fc9bcd1c4");
    public readonly static Guid DestinationDitchesAndDrainageChannels = new("018fe20e-eca0-744b-95bf-b3a85b0e748b");
    public readonly static Guid RoadDrainage = new("018fe20f-d700-7097-bdf0-5d88714b5528");
    public readonly static Guid DestinationNotSure = new("018fe210-c160-7359-97f0-fdd430ed229c");
}

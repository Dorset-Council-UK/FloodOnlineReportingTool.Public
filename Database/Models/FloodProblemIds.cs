namespace FloodOnlineReportingTool.Database.Models;

/// <summary>
/// The flood problem Id's.
/// Helps ensure consistency and allows easier comparison.
/// </summary>
public static class FloodProblemIds
{
    // Primary cause Id's
    public readonly static Guid PrimaryCauseRiver = new("018fe08b-a800-7cb2-aa1b-f39aefc48152");
    public readonly static Guid PrimaryCauseStreamOrWatercourse = new("018fe08c-9260-77ad-9e3a-418bb047ff5d");
    public readonly static Guid LakeOrReservoir = new("018fe08d-7cc0-70ce-89e3-78360019c9b7");
    public readonly static Guid PrimaryCauseTheSea = new("018fe08e-6720-70ea-9a72-bf9aa0de312b");
    public readonly static Guid PrimaryCauseDitchesAndDrainageChannels = new("018fe08f-5180-744d-ab20-b58bf6d04fbb");
    public readonly static Guid WaterRisingOutOfTheGround = new("018fe090-3be0-7d28-b73d-ba671aa61208");
    public readonly static Guid FoulDrainageSewerage = new("018fe091-2640-7a8c-ab1e-5c762268a1d7");
    public readonly static Guid SurfaceWaterDrainage = new("018fe092-10a0-7858-8987-1f84395524b7");
    public readonly static Guid BlockedRoadDrainage = new("018fe092-fb00-7a34-a8c9-bdac73a1acaa");
    public readonly static Guid BridgeOrCulvert = new("018fe093-e560-74dc-ac18-f4949190dce3");
    public readonly static Guid WavesCausedByVehicles = new("018fe094-cfc0-7156-99f5-2fc20e9e19ea");
    public readonly static Guid RainwaterFlowingOverTheGround = new("018fe095-ba20-7234-b73b-ff8e340dd9fc");
    public readonly static Guid PrimaryCauseNotSure = new("018fe096-a480-70d8-91f4-03504bcf926c");

    // Secondary cause Id's
    public readonly static Guid RunoffFromRoad = new("018fe0c2-9680-78b5-a5fd-ca2bb2ddd0e3");
    public readonly static Guid RunoffFromPrivateRoad = new("018fe0c3-80e0-7a22-a8d4-36ad1e5dd626");
    public readonly static Guid RunoffFromTrackOrPath = new("018fe0c4-6b40-7441-afbb-381e233f4906");
    public readonly static Guid RunoffFromAgriculturalLand = new("018fe0c5-55a0-7991-8ee8-1df41519d18e");
    public readonly static Guid RunoffFromOtherProperty = new("018fe0c6-4000-7e95-84d4-1ad96cf4f598");
    public readonly static Guid SecondaryCauseNotSure = new("018fe0c7-2a60-7983-b7c3-afa68072aa5f");

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

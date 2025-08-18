using FloodOnlineReportingTool.Database.Models;

namespace FloodOnlineReportingTool.Database.Seed;

/// <summary>
///     <para>The initial data to seed the database with. This is applied in the migration.</para>
///     <para>"you should use explicit Guids when seeding with HasData, never generated ones" <see href="https://learn.microsoft.com/en-us/archive/msdn-magazine/2018/august/data-points-deep-dive-into-ef-core-hasdata-seeding#what-about-private-setters">Deep Dive into EF Core HasData Seeding</see></para>
/// </summary>
internal static class InitialData
{
    private enum CountyAdminId
    {
        Devon = 23147,
        EastDevon = 22713,
        WestDevon = 22767,
        MidDevon = 22902,
        NorthDevon = 22933,
        Cornwall = 43750,
        BournemouthChristchurchPoole = 175926,
        Dorset = 175988,
    }

    internal static FloodAuthority[] FloodAuthorityData()
    {
        return [
            new FloodAuthority(FloodAuthorityIds.LeadLocalFloodAuthority, "LLFA", "Lead Local Flood Authority"),
            new FloodAuthority(FloodAuthorityIds.EnvironmentAgency, "EA", "Environment Agency"),
            new FloodAuthority(FloodAuthorityIds.WaterAuthority, "Water", "Water Authority"),
            new FloodAuthority(FloodAuthorityIds.GasBoard, "Gas", "Gas Board"),
            new FloodAuthority(FloodAuthorityIds.ElectricityBoard, "Electric", "Electricity Board"),
            new FloodAuthority(FloodAuthorityIds.CATRespond, "CAT Respond", "Emergency responders including resilence direct, government departments and blue light"),
            new FloodAuthority(FloodAuthorityIds.Voluntary, "Voluntary", "Community groups and voluntary sector"),
        ];
    }

    internal static FloodAuthorityFloodProblem[] FloodAuthorityFloodProblemsData()
    {
        // Lead Local Flood Authority flood problems. They are notified for all flood problems.
        // Seeing as all the FloodProblems are setup in the static FloodProblemData method, if we use that, we don't ever miss a flood problem.
        var leadLocalFloodAuthorityProblems = FloodProblemData()
            .Select(fp => new FloodAuthorityFloodProblem(FloodAuthorityIds.LeadLocalFloodAuthority, fp.Id))
            .ToArray();

        return [
            .. leadLocalFloodAuthorityProblems,

            // Environment Agency flood problems
            new FloodAuthorityFloodProblem(FloodAuthorityIds.EnvironmentAgency, FloodProblemIds.PrimaryCauseRiver),
            new FloodAuthorityFloodProblem(FloodAuthorityIds.EnvironmentAgency, FloodProblemIds.PrimaryCauseStreamOrWatercourse),
            new FloodAuthorityFloodProblem(FloodAuthorityIds.EnvironmentAgency, FloodProblemIds.LakeOrReservoir),
            new FloodAuthorityFloodProblem(FloodAuthorityIds.EnvironmentAgency, FloodProblemIds.PrimaryCauseTheSea),
            new FloodAuthorityFloodProblem(FloodAuthorityIds.EnvironmentAgency, FloodProblemIds.WaterRisingOutOfTheGround),

            // Water Authority flood problems
            new FloodAuthorityFloodProblem(FloodAuthorityIds.WaterAuthority, FloodProblemIds.FoulDrainageSewerage),
            new FloodAuthorityFloodProblem(FloodAuthorityIds.WaterAuthority, FloodProblemIds.SurfaceWaterDrainage),

            // Gas Board flood problems
            // Gas and Electric are notified for groundwater due to risks to equipment / longer term nature of flooding but they have no direct inferred responsibility
            new FloodAuthorityFloodProblem(FloodAuthorityIds.GasBoard, FloodProblemIds.WaterRisingOutOfTheGround),

            // Electricity Board flood problems
            new FloodAuthorityFloodProblem(FloodAuthorityIds.ElectricityBoard, FloodProblemIds.WaterRisingOutOfTheGround),

            // CAT Respond flood problems
            // CAT responders are notified for rivers / lakes / reservoirs in case it is a larger breach.
            // These can cause larger incidents but others should be maually assigned where it is appropriate to escalate an incident to a multi-agency response or debrief. 
            new FloodAuthorityFloodProblem(FloodAuthorityIds.CATRespond, FloodProblemIds.PrimaryCauseRiver),
            new FloodAuthorityFloodProblem(FloodAuthorityIds.CATRespond, FloodProblemIds.LakeOrReservoir),
        ];
    }

    internal static FloodImpact[] FloodImpactData()
    {
        return [
            // Property Type
            new FloodImpact(FloodImpactIds.Residential, FloodImpactCategory.PropertyType, "Residential", FloodImpactPriority.None, 1),
            new FloodImpact(FloodImpactIds.Commercial, FloodImpactCategory.PropertyType, "Commercial", FloodImpactPriority.None, 2),
            new FloodImpact(FloodImpactIds.PropertyTypeOther, FloodImpactCategory.PropertyType, "Other", FloodImpactPriority.None, 3),
            new FloodImpact(FloodImpactIds.PropertyTypeNotSpecified, FloodImpactCategory.PropertyType, "Not Specified", FloodImpactPriority.None, 99),

            // Priority
            new FloodImpact(FloodImpactIds.Building, "Priority", "Building", FloodImpactPriority.Internal, 1),
            new FloodImpact(FloodImpactIds.Grounds, "Priority", "Grounds", FloodImpactPriority.External, 2),
            new FloodImpact(FloodImpactIds.Both, "Priority", "Both", FloodImpactPriority.Both, 3),
            new FloodImpact(FloodImpactIds.Unknown, "Priority", "Unknown", FloodImpactPriority.Other, 4),
            new FloodImpact(FloodImpactIds.PriorityNotSpecified, "Priority", "Not Specified", FloodImpactPriority.Other, 9),

            // Zone Residential
            new FloodImpact(FloodImpactIds.InsideLivingArea, FloodImpactCategory.Residential, "Inside living area", FloodImpactPriority.Internal, 1),
            new FloodImpact(FloodImpactIds.MobileHomeCaravan, FloodImpactCategory.Residential, "Mobile Home / Caravan", FloodImpactPriority.Internal, 2),
            new FloodImpact(FloodImpactIds.Basement, FloodImpactCategory.Residential, "Basement / Cellar", FloodImpactPriority.Internal, 3),
            new FloodImpact(FloodImpactIds.GarageAttached, FloodImpactCategory.Residential, "Garage attached to property", FloodImpactPriority.Internal, 4),
            new FloodImpact(FloodImpactIds.ZoneRUnderFloorboards, FloodImpactCategory.Residential, "Under floorboards", FloodImpactPriority.Internal, 5),
            new FloodImpact(FloodImpactIds.ZoneRAgainstWall, FloodImpactCategory.Residential, "Against property wall",  FloodImpactPriority.External, 6),
            new FloodImpact(FloodImpactIds.PropertyAccess, FloodImpactCategory.Residential, "Property Access",  FloodImpactPriority.External, 7),
            new FloodImpact(FloodImpactIds.ZoneROutbuilding, FloodImpactCategory.Residential, "Outbuilding(s)",  FloodImpactPriority.External, 8),
            new FloodImpact(FloodImpactIds.Garden, FloodImpactCategory.Residential, "Garden",  FloodImpactPriority.External, 9),
            new FloodImpact(FloodImpactIds.ZoneRRoad, FloodImpactCategory.Residential, "Road",  FloodImpactPriority.External, 10),
            new FloodImpact(FloodImpactIds.ZoneRNotSure, FloodImpactCategory.Residential, "Not Sure", FloodImpactPriority.Other, 99),

            // Zone Commercial
            new FloodImpact(FloodImpactIds.InsideBuilding, FloodImpactCategory.Commercial, "Inside building", FloodImpactPriority.Internal, 1),
            new FloodImpact(FloodImpactIds.BelowGroundLevelFloors, FloodImpactCategory.Commercial, "Below ground level floors", FloodImpactPriority.Internal, 2),
            new FloodImpact(FloodImpactIds.ZoneCUnderFloorboards, FloodImpactCategory.Commercial, "Under floorboards", FloodImpactPriority.Internal, 3),
            new FloodImpact(FloodImpactIds.ZoneCAgainstWall, FloodImpactCategory.Commercial, "Against property wall",  FloodImpactPriority.External, 4),
            new FloodImpact(FloodImpactIds.ZoneCOutbuilding, FloodImpactCategory.Commercial, "Outbuilding(s)",  FloodImpactPriority.External, 5),
            new FloodImpact(FloodImpactIds.FieldsBusinessLand, FloodImpactCategory.Commercial, "Fields / Business Land",  FloodImpactPriority.External, 6),
            new FloodImpact(FloodImpactIds.CarPark, FloodImpactCategory.Commercial, "Car Park",  FloodImpactPriority.External, 7),
            new FloodImpact(FloodImpactIds.Access, FloodImpactCategory.Commercial, "Access",  FloodImpactPriority.External, 8),
            new FloodImpact(FloodImpactIds.ZoneCRoad, FloodImpactCategory.Commercial, "Road",  FloodImpactPriority.External, 9),
            new FloodImpact(FloodImpactIds.ZoneCNotSure, FloodImpactCategory.Commercial, "Not Sure", FloodImpactPriority.Other, 99),

            // Zone-E
            new FloodImpact(FloodImpactIds.ServicesNotAffected, "Service Impact", "Services not affected", FloodImpactPriority.None, 1),
            new FloodImpact(FloodImpactIds.PrivateSewer, "Service Impact", "Private Sewer", FloodImpactPriority.None, 2),
            new FloodImpact(FloodImpactIds.MainsSewer, "Service Impact", "Mains Sewer", FloodImpactPriority.None, 3),
            new FloodImpact(FloodImpactIds.WaterSupply, "Service Impact", "Water Supply", FloodImpactPriority.None, 4),
            new FloodImpact(FloodImpactIds.Gas, "Service Impact", "Gas", FloodImpactPriority.None, 5),
            new FloodImpact(FloodImpactIds.Electricity, "Service Impact", "Electricity", FloodImpactPriority.None, 6),
            new FloodImpact(FloodImpactIds.Phoneline, "Service Impact", "Phoneline", FloodImpactPriority.None, 7),
            new FloodImpact(FloodImpactIds.ZoneENotSure, "Service Impact", "Not Sure", FloodImpactPriority.None, 99),

            // Community Impact
            new FloodImpact(FloodImpactIds.AllRoadAccessBlocked, FloodImpactCategory.CommunityImpact, "All road access blocked", FloodImpactPriority.None, 1),
            new FloodImpact(FloodImpactIds.SomeRoadAccessBlocked, FloodImpactCategory.CommunityImpact, "Some road access blocked", FloodImpactPriority.None, 2),
            new FloodImpact(FloodImpactIds.NoAccessToPlaceOfWork, FloodImpactCategory.CommunityImpact, "No access to place of work", FloodImpactPriority.None, 3),
            new FloodImpact(FloodImpactIds.PublicTransportDisrupted, FloodImpactCategory.CommunityImpact, "Public transport disrupted", FloodImpactPriority.None, 4),
            new FloodImpact(FloodImpactIds.LocalShopClosed, FloodImpactCategory.CommunityImpact, "Local shop(s) closed", FloodImpactPriority.None, 5),
            new FloodImpact(FloodImpactIds.CommunityImpactNotSure, FloodImpactCategory.CommunityImpact, "Not Sure", FloodImpactPriority.None, 99),

            // Impact Duration
            new FloodImpact(FloodImpactIds.UseNotDisrupted, "Impact Duration", "Use not disrupted", FloodImpactPriority.None, 1),
            new FloodImpact(FloodImpactIds.UpToOneWeek, "Impact Duration", "Up to 1 week", FloodImpactPriority.None, 2),
            new FloodImpact(FloodImpactIds.OneWeekToOneMonth, "Impact Duration", "1 week to 1 month", FloodImpactPriority.None, 3),
            new FloodImpact(FloodImpactIds.OneMonthToSixMonths, "Impact Duration", "1 month to 6 months", FloodImpactPriority.None, 4),
            new FloodImpact(FloodImpactIds.GreaterThanSixMonths, "Impact Duration", ">6 months", FloodImpactPriority.None, 5),
            new FloodImpact(FloodImpactIds.StillUnable, "Impact Duration", "Still unable", FloodImpactPriority.None, 6),
            new FloodImpact(FloodImpactIds.ImpactDurationNotSure, "Impact Duration", "Not Sure", FloodImpactPriority.None, 99),
        ];
    }

    internal static FloodMitigation[] FloodMitigationData()
    {
        return [
            // Action Taken
            new FloodMitigation(FloodMitigationIds.NoActionTaken, FloodMitigationCategory.ActionsTaken, "No Action Taken", 99),
            new FloodMitigation(FloodMitigationIds.Sandbags, FloodMitigationCategory.ActionsTaken, "Sandbags", 2),
            new FloodMitigation(FloodMitigationIds.SandlessSandbag, FloodMitigationCategory.ActionsTaken, "Sandless Sandbag", 3),
            new FloodMitigation(FloodMitigationIds.BoardsGate, FloodMitigationCategory.ActionsTaken, "Flood Boards / Gate", 4),
            new FloodMitigation(FloodMitigationIds.FloodDoor, FloodMitigationCategory.ActionsTaken, "Flood Door", 5),
            new FloodMitigation(FloodMitigationIds.BackflowValve, FloodMitigationCategory.ActionsTaken, "Back-flow valve", 6),
            new FloodMitigation(FloodMitigationIds.AirBrickCover, FloodMitigationCategory.ActionsTaken, "Air brick cover", 7),
            new FloodMitigation(FloodMitigationIds.PumpedWater, FloodMitigationCategory.ActionsTaken, "Pumped Water", 8),
            new FloodMitigation(FloodMitigationIds.MoveValuables, FloodMitigationCategory.ActionsTaken, "Move Valuables", 9),
            new FloodMitigation(FloodMitigationIds.MoveCar, FloodMitigationCategory.ActionsTaken, "Move Car", 10),
            new FloodMitigation(FloodMitigationIds.OtherAction, FloodMitigationCategory.ActionsTaken, "Other", 11),

            // Help Received From
            new FloodMitigation(FloodMitigationIds.NoHelp, FloodMitigationCategory.HelpReceived, "No Help", 99),
            new FloodMitigation(FloodMitigationIds.NeighboursFamilyHelp, FloodMitigationCategory.HelpReceived, "Neighbours / Family", 2),
            new FloodMitigation(FloodMitigationIds.WardenVolunteerHelp, FloodMitigationCategory.HelpReceived, "Wardens / Volunteers", 3),
            new FloodMitigation(FloodMitigationIds.FirePolice, FloodMitigationCategory.HelpReceived, "Fire and Rescue / Police", 4),
            new FloodMitigation(FloodMitigationIds.EnvironmentAgency, FloodMitigationCategory.HelpReceived, "Environment Agency", 5),
            new FloodMitigation(FloodMitigationIds.Highways, FloodMitigationCategory.HelpReceived, "Highways", 6),
            new FloodMitigation(FloodMitigationIds.LocalAuthority, FloodMitigationCategory.HelpReceived, "Local Authority", 7),
            new FloodMitigation(FloodMitigationIds.FloodlineHelp, FloodMitigationCategory.HelpReceived, "Floodline", 8),

            // Warning Source
            new FloodMitigation(FloodMitigationIds.NoWarning, FloodMitigationCategory.WarningSource, "I did not get a warning", 99),
            new FloodMitigation(FloodMitigationIds.FloodlineWarning, FloodMitigationCategory.WarningSource, "Floodline", 2),
            new FloodMitigation(FloodMitigationIds.Television, FloodMitigationCategory.WarningSource, "Television", 3),
            new FloodMitigation(FloodMitigationIds.Radio, FloodMitigationCategory.WarningSource, "Radio", 4),
            new FloodMitigation(FloodMitigationIds.SocialMediaInternet, FloodMitigationCategory.WarningSource, "Social Media/Internet", 5),
            new FloodMitigation(FloodMitigationIds.WardenVolunteerWarning, FloodMitigationCategory.WarningSource, "Flood Warden/Volunteer", 6),
            new FloodMitigation(FloodMitigationIds.NeighbourWarning, FloodMitigationCategory.WarningSource, "Neighbours", 7),
            new FloodMitigation(FloodMitigationIds.OtherWarning, FloodMitigationCategory.WarningSource, "Other", 8),

            // Flood Warden Awareness
            new FloodMitigation(FloodMitigationIds.BeforeFlooding, FloodMitigationCategory.WardenAwareness, "Before flooding", 1),
            new FloodMitigation(FloodMitigationIds.DuringFlooding, FloodMitigationCategory.WardenAwareness, "During flooding", 2),
            new FloodMitigation(FloodMitigationIds.AfterFlooding, FloodMitigationCategory.WardenAwareness, "After flooding", 3),
            new FloodMitigation(FloodMitigationIds.NotSureFloodWarden, FloodMitigationCategory.WardenAwareness, "What are flood wardens/volunteers?", 4),
        ];
    }

    internal static FloodProblem[] FloodProblemData()
    {
        return [
            // Primary Cause
            new FloodProblem(FloodProblemIds.PrimaryCauseRiver, FloodProblemCategory.PrimaryCause, "River", "Caused by an overflowing main river", 1),
            new FloodProblem(FloodProblemIds.PrimaryCauseStreamOrWatercourse, FloodProblemCategory.PrimaryCause, "Stream / Watercourse", "Caused by an overflowing stream or watercourse (not a main river)", 2),
            new FloodProblem(FloodProblemIds.LakeOrReservoir, FloodProblemCategory.PrimaryCause, "Lake / Reservoirs", "Caused by an overflowing lake or reservoir", 3),
            new FloodProblem(FloodProblemIds.PrimaryCauseTheSea, FloodProblemCategory.PrimaryCause, "The Sea", "Caused by sea water including high tides", 4),
            new FloodProblem(FloodProblemIds.PrimaryCauseDitchesAndDrainageChannels, FloodProblemCategory.PrimaryCause, "Ditches and drainage channels", "Caused by blocked ditches or channels", 5),
            new FloodProblem(FloodProblemIds.WaterRisingOutOfTheGround, FloodProblemCategory.PrimaryCause, "Water rising out of the ground", "The water is coming out of the ground (groundwater)", 6),
            new FloodProblem(FloodProblemIds.FoulDrainageSewerage, FloodProblemCategory.PrimaryCause, "Foul drainage (Sewerage)", "Caused by overwhelmed foul sewer", 7),
            new FloodProblem(FloodProblemIds.SurfaceWaterDrainage, FloodProblemCategory.PrimaryCause, "Surface water drainage", "Caused by overwhelmed drains (not foul/sewer water)", 8),
            new FloodProblem(FloodProblemIds.BlockedRoadDrainage, FloodProblemCategory.PrimaryCause, "Blocked road drainage", "Caused by blocked road drainage", 9),
            new FloodProblem(FloodProblemIds.BridgeOrCulvert, FloodProblemCategory.PrimaryCause, "Bridge / culvert", "Caused by an issue with a bridge or underground water channel (culvert)", 10),
            new FloodProblem(FloodProblemIds.WavesCausedByVehicles, FloodProblemCategory.PrimaryCause, "Waves caused by vehicles", "Waves caused by vehicles", 11),
            new FloodProblem(FloodProblemIds.RainwaterFlowingOverTheGround, FloodProblemCategory.PrimaryCause, "Rainwater flowing over the ground", "Rainwater flowing over the ground", 12),
            new FloodProblem(FloodProblemIds.PrimaryCauseNotSure, FloodProblemCategory.PrimaryCause, "Not Sure", "I don't know where the water came from", 99),

            // Secondary Cause
            new FloodProblem(FloodProblemIds.RunoffFromRoad, FloodProblemCategory.SecondaryCause, "Runoff from road", "Water flowing from an council maintained road", 1),
            new FloodProblem(FloodProblemIds.RunoffFromPrivateRoad, FloodProblemCategory.SecondaryCause, "Runoff from private road", "Water flowing from a private road", 2),
            new FloodProblem(FloodProblemIds.RunoffFromTrackOrPath, FloodProblemCategory.SecondaryCause, "Runoff from track/path", "Water flowing from a track or footpath", 3),
            new FloodProblem(FloodProblemIds.RunoffFromAgriculturalLand, FloodProblemCategory.SecondaryCause, "Runoff from agricultural land", "Water flowing from agricultural land (fields)", 4),
            new FloodProblem(FloodProblemIds.RunoffFromOtherProperty, FloodProblemCategory.SecondaryCause, "Runoff from other property", "Water flowing from a neighbouring property", 5),
            new FloodProblem(FloodProblemIds.SecondaryCauseNotSure, FloodProblemCategory.SecondaryCause, "Not Sure", "I don't know which option is right", 99),

            // Investigation form, appearance
            new FloodProblem(FloodProblemIds.Clear, FloodProblemCategory.Appearance, "Clear", "The water was clear / clean", 1),
            new FloodProblem(FloodProblemIds.Muddy, FloodProblemCategory.Appearance, "Muddy", "The water was muddy / cloudy", 2),
            new FloodProblem(FloodProblemIds.PollutedWithSewage, FloodProblemCategory.Appearance, "Polluted with sewage", "The water had sewage in it", 3),

            // Investigation form, flood water onset
            new FloodProblem(FloodProblemIds.Suddenly, FloodProblemCategory.WaterOnset, "Suddenly", "The water came rapidly (flash flooding)", 1),
            new FloodProblem(FloodProblemIds.Gradually, FloodProblemCategory.WaterOnset, "Gradually", "The water rose gradually", 2),

            // Investigation form, water speed
            new FloodProblem(FloodProblemIds.Fast, FloodProblemCategory.Speed, "Fast", "The water was flowing fast", 1),
            new FloodProblem(FloodProblemIds.Slow, FloodProblemCategory.Speed, "Slow (walking pace)", "The water was flowing slowly", 2),
            new FloodProblem(FloodProblemIds.Still, FloodProblemCategory.Speed, "Still", "The water was not flowing / still", 3),

            // Duration - Water duration replaced with int for number of days and boolean for ongoing however we still use the radios as a helper
            new FloodProblem(FloodProblemIds.Duration1, FloodProblemCategory.Duration, "1", "Less than 1 hour", 1),
            new FloodProblem(FloodProblemIds.Duration24, FloodProblemCategory.Duration, "24", "1 hour to 24 hours", 2),
            new FloodProblem(FloodProblemIds.Duration168, FloodProblemCategory.Duration, "168", "24 hours to 1 week", 3),
            new FloodProblem(FloodProblemIds.Duration744, FloodProblemCategory.Duration, "744", "More than 1 week", 4),
            new FloodProblem(FloodProblemIds.DurationKnown, FloodProblemCategory.Duration, null, "I know how many days/hours", 5),
            new FloodProblem(FloodProblemIds.DurationNotSure, FloodProblemCategory.Duration, "48", "Not Sure", 99),

            // Water Entry
            new FloodProblem(FloodProblemIds.Door, FloodProblemCategory.Entry, "Door", "The water came through a door", 1),
            new FloodProblem(FloodProblemIds.Windows, FloodProblemCategory.Entry, "Windows", "The water came through a window", 2),
            new FloodProblem(FloodProblemIds.Airbrick, FloodProblemCategory.Entry, "Airbrick", "The water came through an airbrick or vent", 3),
            new FloodProblem(FloodProblemIds.Walls, FloodProblemCategory.Entry, "Walls", "The water came through the walls", 4),
            new FloodProblem(FloodProblemIds.ThroughFloor, FloodProblemCategory.Entry, "Through Floor", "The water came up through the floor", 5),
            new FloodProblem(FloodProblemIds.ExternalOnly, FloodProblemCategory.Entry, "External Only", "The water came up to the property but did not enter", 6),
            new FloodProblem(FloodProblemIds.EntryOther, FloodProblemCategory.Entry, "Other", "None of the options are correct", 7),
            new FloodProblem(FloodProblemIds.EntryNotSure, FloodProblemCategory.Entry, "Not Sure", "I don't know which option is right", 99),

            // Water Destination
            new FloodProblem(FloodProblemIds.DestinationRiver, FloodProblemCategory.Destination, "River", "The flood water was flowing into a main river", 1),
            new FloodProblem(FloodProblemIds.DestinationStreamOrWatercourse, FloodProblemCategory.Destination, "Stream / Watercourse", "The flood water was flowing into a stream of watercourse (not a main river)", 2),
            new FloodProblem(FloodProblemIds.DestinationTheSea, FloodProblemCategory.Destination, "The Sea", "The flood water was flowing into the sea", 3),
            new FloodProblem(FloodProblemIds.DestinationDitchesAndDrainageChannels, FloodProblemCategory.Destination, "Ditches and drainage channels", "The flood water was flowing into a ditch or channel", 4),
            new FloodProblem(FloodProblemIds.RoadDrainage, FloodProblemCategory.Destination, "Road drainage", "The flood water was flowing into road drains", 5),
            new FloodProblem(FloodProblemIds.DestinationNotSure, FloodProblemCategory.Destination, "Not Sure", "I don't know which option is right", 99),
        ];
    }

    internal static Organisation[] OrganisationData()
    {
        var updatedDate = new DateTimeOffset(2024, 06, 1, 0, 0, 0, TimeSpan.Zero);

        return [
            new Organisation
            {
                Id = OrganisationIds.Dorset,
                Name = "Dorset Council",
                Description = "Dorset Council is the Lead Local Flood Authority and we are consulted in relation to planning applications and other statutory duties relating to flooding. We also undertaken Section 19 flood investigations where a flood incident meets our significance criteria.",
                Logo = new Uri("https://fort-uk.dorsetcouncil.gov.uk/medialib/orglogo/Dorset_Council_logo_colour.jpg"),
                DataProtectionStatement = "<p>Dorset County Council is defined as a Lead Local Flood Authority under the Flood and Water Management Act of 2010. We collect data via FORT to inform our Section 19 flood reports and as a starting point for detailed flood investigations and property level grants and protection schemes.</p><p>We may download and keep a copy of personal data submitted via the FORT website to use within our standard working practices.This data will only be used for the management of particular flood reports and personal data will be removed from the systems once there is no longer a need to contact you regarding your submission and once you agree that you no longer wish to provide updates to the record.</p>",
                EmergencyPlanning = "Notification sent to the emergency planners who will endeavour to review the record within 2 hours.",
                FloodAuthorityId = FloodAuthorityIds.LeadLocalFloodAuthority,
                GettingInTouch = "If you require further advice please contact Dorset Direct on 01305 221000 and ask to speak to a member of the Flood Risk Management Team.",
                SubmissionReply = "<p>The Dorset Council flood risk team will be notified of your flood report and will review it in line with their prioritisation criteria.</p><p>The FORT system provides a convenient single location to notify multiple organisations of property level flooding as there is no single body responsible for <a href=\"http://www.local.gov.uk/local-flood-risk-management/-/journal_content/56/10180/3572186/ARTICLE\" target=\"_blank\">managing flood risk in the UK</a>.The FORT system aims to ensure that reports of flooding are passed on to the correct organisation(s).</p><p>The management of any further actions will be from the organisation(s) receiving the record. It is important to realise that each recipient will apply their own plans and policies to the records received and prioritise reports accordingly. You may not receive a response and your record will not trigger any emergency action(s). The level of feedback added to your record, by partner organisations using the FORT system, shall again depend upon their own policies.</p>",
                Website = new Uri("https://www.dorsetcouncil.gov.uk"),
                LastUpdatedUtc = updatedDate,
            },
            new Organisation
            {
                Id = OrganisationIds.Wessex,
                Name = "Environment Agency (Wessex)",
                Description = "The Environment Agency is a Risk Authority and we are consulted in relation to planning applications and other statutory duties relating to flooding from main rivers and the sea.",
                Logo = new Uri("https://assets.publishing.service.gov.uk/government/uploads/system/uploads/organisation/logo/199/environment-agency-logo-480w.png"),
                FloodAuthorityId = FloodAuthorityIds.EnvironmentAgency,
                Website = new Uri("https://www.gov.uk/government/organisations/environment-agency"),
                LastUpdatedUtc = updatedDate,
            },
            new Organisation
            {
                Id = OrganisationIds.DevonCornwall,
                Name = "Environment Agency (Devon & Cornwall)",
                Description = "The Environment Agency is a Risk Authority and we are consulted in relation to planning applications and other statutory duties relating to flooding from main rivers and the sea.",
                Logo = new Uri("https://assets.publishing.service.gov.uk/government/uploads/system/uploads/organisation/logo/199/environment-agency-logo-480w.png"),
                FloodAuthorityId = FloodAuthorityIds.EnvironmentAgency,
                Website = new Uri("https://www.gov.uk/government/organisations/environment-agency"),
                LastUpdatedUtc = updatedDate,
            },
        ];
    }

    internal static RecordStatus[] RecordStatusData()
    {
        return [

            // Flood report status
            new RecordStatus(RecordStatusIds.MarkedForDeletion, RecordStatusCategory.FloodReportStatus, "Marked for Deletion", 99, "The record is marked for deletion and will be removed from the system in 48 hours"),
            new RecordStatus(RecordStatusIds.New, RecordStatusCategory.FloodReportStatus, "New", 1, "This is a new record that has not been viewed yet"),
            new RecordStatus(RecordStatusIds.Viewed, RecordStatusCategory.FloodReportStatus, "Viewed", 2, "This record has been viewed but no action has been taken yet"),
            new RecordStatus(RecordStatusIds.ActionNeeded, RecordStatusCategory.FloodReportStatus, "Action needed", 3, "Action is needed on this record and needs to be reviewed"),
            new RecordStatus(RecordStatusIds.ActionCompleted, RecordStatusCategory.FloodReportStatus, "Action completed", 4, "Action has been completed on this record"),
            new RecordStatus(RecordStatusIds.Error, RecordStatusCategory.FloodReportStatus, "Error", 5, "This record has an error and needs to be reviewed"),

            // Area
            new RecordStatus(RecordStatusIds.PreparePhase, RecordStatusCategory.Phase, "Prepare Phase", 1, "A forecast of/imminent risk of buildings/land being flooded"),
            new RecordStatus(RecordStatusIds.ResponsePhase, RecordStatusCategory.Phase, "Response Phase", 2, "Buildings/land are currently flooded"),
            new RecordStatus(RecordStatusIds.RecoveryPhase, RecordStatusCategory.Phase, "Recovery Phase", 3, "A recent flood event where buildings/land are no longer flooded but remedial work to properties is on-going"),
            new RecordStatus(RecordStatusIds.AnalysePhase, RecordStatusCategory.Phase, "Analyse Phase", 4, "A past flood event where homes/businesses/land are no longer affected"),

            // Area Flood Status
            new RecordStatus(RecordStatusIds.FloodExpectedNoFlood, RecordStatusCategory.AreaFloodStatus, "Flood Expected: No Flood", 1),
            new RecordStatus(RecordStatusIds.FloodExpectedHelpGiven, RecordStatusCategory.AreaFloodStatus, "Flood Expected: Help Given", 2),
            new RecordStatus(RecordStatusIds.PropertiesAffected, RecordStatusCategory.AreaFloodStatus, "Properties Affected", 3),
            new RecordStatus(RecordStatusIds.PropertiesAffectedHelpGiven, RecordStatusCategory.AreaFloodStatus, "Properties Affected: Help Given", 4),
            new RecordStatus(RecordStatusIds.BuildingsFlooded, RecordStatusCategory.AreaFloodStatus, "Buildings Flooded", 5),
            new RecordStatus(RecordStatusIds.BuildingsFloodedHelpGiven, RecordStatusCategory.AreaFloodStatus, "Buildings Flooded: Help Given", 6),
            new RecordStatus(RecordStatusIds.NoFloodingOccurred, RecordStatusCategory.AreaFloodStatus, "No Flooding Occurred", 7),

            // Validation
            new RecordStatus(RecordStatusIds.Unconfirmed, RecordStatusCategory.Validation, "Unconfirmed", 1),
            new RecordStatus(RecordStatusIds.Validated, RecordStatusCategory.Validation, "Validated", 2),

            // Section19 Status
            new RecordStatus(RecordStatusIds.NoSection19, RecordStatusCategory.Section19, "No Section 19 report", 1),
            new RecordStatus(RecordStatusIds.Section19Required, RecordStatusCategory.Section19, "Section 19 report required", 2),
            new RecordStatus(RecordStatusIds.Section19InProgress, RecordStatusCategory.Section19, "Section 19 report in progress", 3),
            new RecordStatus(RecordStatusIds.Section19Included, RecordStatusCategory.Section19, "Included in Section 19 report", 4),

            // Data Protection
            new RecordStatus(RecordStatusIds.NotAcknowledged, RecordStatusCategory.DataProtection, "Not yet acknowledged", 1),
            new RecordStatus(RecordStatusIds.Agreed, RecordStatusCategory.DataProtection, "Agreed", 2),

            // Yes / No / Not Sure
            new RecordStatus(RecordStatusIds.Yes, RecordStatusCategory.General, "Yes", 1),
            new RecordStatus(RecordStatusIds.No, RecordStatusCategory.General, "No", 2),
            new RecordStatus(RecordStatusIds.NotSure, RecordStatusCategory.General, "Not Sure", 3),
        ];
    }

    internal static FloodResponsibility[] FloodResponsibilityData()
    {
        var lookupDate = new DateOnly(2024, 06, 24);

        return [
            // Dorset
            new FloodResponsibility
            {
                OrganisationId = OrganisationIds.Dorset,
                AdminUnitId = (int)CountyAdminId.Dorset,
                Name = "Dorset",
                Description = "Unitary Authority",
                LookupDate = lookupDate,
            },
            new FloodResponsibility
            {
                OrganisationId = OrganisationIds.Dorset,
                AdminUnitId = (int)CountyAdminId.BournemouthChristchurchPoole,
                Name = "Bournemouth, Christchurch and Poole",
                Description = "Unitary Authority",
                LookupDate = lookupDate,
            },

            // Devon & Cornwall
            new FloodResponsibility
            {
                OrganisationId = OrganisationIds.DevonCornwall,
                AdminUnitId = (int)CountyAdminId.Dorset,
                Name = "Dorset",
                Description ="Unitary Authority",
                LookupDate = lookupDate,
            },
            new FloodResponsibility
            {
                OrganisationId = OrganisationIds.DevonCornwall,
                AdminUnitId = (int)CountyAdminId.BournemouthChristchurchPoole,
                Name = "Bournemouth, Christchurch and Poole",
                Description = "Unitary Authority",
                LookupDate = lookupDate,
            },
            new FloodResponsibility
            {
                OrganisationId = OrganisationIds.DevonCornwall,
                AdminUnitId = (int)CountyAdminId.Devon,
                Name = "Devon County",
                Description = "County",
                LookupDate = lookupDate,
            },
            new FloodResponsibility
            {
                OrganisationId = OrganisationIds.DevonCornwall,
                AdminUnitId = (int)CountyAdminId.NorthDevon,
                Name = "North Devon District",
                Description = "District",
                LookupDate = lookupDate,
            },
            new FloodResponsibility
            {
                OrganisationId = OrganisationIds.DevonCornwall,
                AdminUnitId = (int)CountyAdminId.WestDevon,
                Name = "West Devon District (B)",
                Description = "District",
                LookupDate = lookupDate,
            },
            new FloodResponsibility
            {
                OrganisationId = OrganisationIds.DevonCornwall,
                AdminUnitId = (int)CountyAdminId.MidDevon,
                Name = "Mid Devon District",
                Description = "District",
                LookupDate = lookupDate,
            },
            new FloodResponsibility
            {
                OrganisationId = OrganisationIds.DevonCornwall,
                AdminUnitId = (int)CountyAdminId.EastDevon,
                Name = "East Devon District",
                Description = "District",
                LookupDate = lookupDate,
            },
            new FloodResponsibility
            {
                OrganisationId = OrganisationIds.DevonCornwall,
                AdminUnitId = (int)CountyAdminId.Cornwall,
                Name = "Cornwall",
                Description = "Unitary Authority",
                LookupDate = lookupDate,
            },
        ];
    }
}

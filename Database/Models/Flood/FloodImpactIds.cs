namespace FloodOnlineReportingTool.Database.Models.Flood;

/// <summary>
/// The flood impact Id's.
/// Helps ensure consistency and allows easier comparison.
/// </summary>
public static class FloodImpactIds
{
    // Property type Id's
    public readonly static Guid Residential = new("018fd63e-f000-732d-9d84-5f1f4f54f3bd");
    public readonly static Guid Commercial = new("018fd63f-da60-7c6c-9a7c-a197c733e7ea");
    public readonly static Guid PropertyTypeOther = new("018fd640-c4c0-7e7c-aa03-d4d09a3e2e80");
    public readonly static Guid PropertyTypeNotSpecified = new("018fd641-af20-74f2-9576-38b0dd12f330");

    // Priority Id's
    public readonly static Guid Building = new("018fd675-de80-7b96-954f-12f13f833dbc");
    public readonly static Guid Grounds = new("018fd676-c8e0-7d18-b734-63be2020c56c");
    public readonly static Guid Both = new("018fd677-b340-750f-8f91-00a7d5ac4065");
    public readonly static Guid Unknown = new("018fd678-9da0-7703-8109-866e5b539d83");
    public readonly static Guid PriorityNotSpecified = new("018fd679-8800-7b74-9606-7e0e238753d5");

    // Zone-R Id's
    public readonly static Guid InsideLivingArea = new("018fd6ac-cd00-7293-abb0-f3d05840e090");
    public readonly static Guid MobileHomeCaravan = new("018fd6ad-b760-79d7-b095-74d9baa9ef5d");
    public readonly static Guid Basement = new("018fd6ae-a1c0-70f4-97d1-b26b0302d54d");
    public readonly static Guid GarageAttached = new("018fd6af-8c20-75c0-ba44-eb76e844007a");
    public readonly static Guid ZoneRUnderFloorboards = new("018fd6b0-7680-711f-84b9-4d9f961bc82b");
    public readonly static Guid ZoneRAgainstWall = new("018fd6b1-60e0-734c-8bb1-8d483f863cfd");
    public readonly static Guid PropertyAccess = new("018fd6b2-4b40-7780-8b9a-51ebe0c8d5a6");
    public readonly static Guid ZoneROutbuilding = new("018fd6b3-35a0-7490-896d-02fcfb1709af");
    public readonly static Guid Garden = new("018fd6b4-2000-7a42-921d-63fd1c3c526c");
    public readonly static Guid ZoneRRoad = new("018fd6b5-0a60-77a4-9d68-9360a287b95f");
    public readonly static Guid ZoneRNotSure = new("018fda40-5400-793d-b3c2-e058c29ef1cb");

    // Zone-C Id's
    public readonly static Guid InsideBuilding = new("018fd6e3-bb80-7874-a578-b56a8f6fa390");
    public readonly static Guid BelowGroundLevelFloors = new("018fd6e4-a5e0-7389-aae7-7b5a64b6d35e");
    public readonly static Guid ZoneCUnderFloorboards = new("018fd6e5-9040-7a02-a8e2-64d1acd5941d");
    public readonly static Guid ZoneCAgainstWall = new("018fd6e6-7aa0-7a90-ad06-e7a71d76f6dc");
    public readonly static Guid ZoneCOutbuilding = new("018fd6e7-6500-76f2-94b1-e1671217fd29");
    public readonly static Guid FieldsBusinessLand = new("018fd6e8-4f60-76cb-a4f3-d83117b1828c");
    public readonly static Guid CarPark = new("018fd6e9-39c0-7483-a529-7cd3a48fc038");
    public readonly static Guid Access = new("018fd6ea-2420-7b30-b20e-5fccfd98b345");
    public readonly static Guid ZoneCRoad = new("018fd6eb-0e80-78a6-b74e-8a65c9293f90");
    public readonly static Guid ZoneCNotSure = new("018fd6eb-f8e0-7844-b002-405ef83ab875");

    // Service impact Id's / Zone-E Id's
    public readonly static Guid ServicesNotAffected = new("018fd71a-aa00-7ac0-b521-ccf27f194875");
    public readonly static Guid PrivateSewer = new("018fd71b-9460-715b-aa13-d9eabd5b7ef1");
    public readonly static Guid MainsSewer = new("018fd71c-7ec0-7a1b-94a6-c7d7ae52b977");
    public readonly static Guid WaterSupply = new("018fd71d-6920-787b-ab3f-b6f251f4834b");
    public readonly static Guid Gas = new("018fd71e-5380-79a2-8e37-ab4e24f063a2");
    public readonly static Guid Electricity = new("018fd71f-3de0-7551-b3a4-7916759c83fe");
    public readonly static Guid Phoneline = new("018fd720-2840-7273-bfcd-4ce03f7f249e");
    public readonly static Guid ServiceImpactNotSure = new("018fd721-12a0-7341-a0fb-818543c14e0f");

    // Community impact Id's
    public readonly static Guid AllRoadAccessBlocked = new("018fd751-9880-7fe6-812e-3683961317a9");
    public readonly static Guid SomeRoadAccessBlocked = new("018fd752-82e0-7560-8b2f-441c7ff1800a");
    public readonly static Guid NoAccessToPlaceOfWork = new("018fd753-6d40-7327-b7dc-e5286d2a5bf3");
    public readonly static Guid PublicTransportDisrupted = new("018fd754-57a0-7009-b36e-49d223f5515c");
    public readonly static Guid LocalShopClosed = new("018fd755-4200-706e-89da-48876a818c73");
    public readonly static Guid CommunityImpactNotSure = new("018fd756-2c60-7616-a03f-6e03f996cd1f");

    // Impact duration Id's
    public readonly static Guid UseNotDisrupted = new("018fd788-8700-723b-aa01-d93fa589ab4d");
    public readonly static Guid UpToOneWeek = new("018fd789-7160-74da-b17b-871e5de26e3a");
    public readonly static Guid OneWeekToOneMonth = new("018fd78a-5bc0-77fe-9930-fe113cc34dc9");
    public readonly static Guid OneMonthToSixMonths = new("018fd78b-4620-72d5-bb2c-6eb8edb20691");
    public readonly static Guid GreaterThanSixMonths = new("018fd78c-3080-7d4e-88ef-4b3013a8bb91");
    public readonly static Guid StillUnable = new("018fd78d-1ae0-7fb3-bc3e-a9adc9b3dd7f");
    public readonly static Guid ImpactDurationNotSure = new("018fd78e-0540-7b80-ac80-b58c96edc173");
}

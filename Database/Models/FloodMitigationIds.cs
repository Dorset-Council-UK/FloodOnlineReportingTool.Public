namespace FloodOnlineReportingTool.Database.Models;

/// <summary>
/// The flood mitigation Id's.
/// Helps ensure consistency and allows easier comparison.
/// </summary>
public static class FloodMitigationIds
{
    // Actions taken Id's
    public readonly static Guid NoActionTaken =new("018fdb65-4c00-7552-bcf3-0a398a590464");
    public readonly static Guid Sandbags = new("018fdb66-3660-70ed-af58-61a95da37750");
    public readonly static Guid SandlessSandbag = new("018fdb67-20c0-7c09-8aa3-818bc80648f6");
    public readonly static Guid BoardsGate = new("018fdb68-0b20-7840-bbb4-4cc1120720ac");
    public readonly static Guid FloodDoor = new("018fdb68-f580-761f-8f45-805d18c65823");
    public readonly static Guid BackflowValve = new("018fdb69-dfe0-735a-ba5a-389eb2f5f753");
    public readonly static Guid AirBrickCover = new("018fdb6a-ca40-7e06-b6cd-0dab09a39e90");
    public readonly static Guid PumpedWater = new("018fdb6b-b4a0-77c5-9721-e3a1cac011fa");
    public readonly static Guid MoveValuables = new("018fdb6c-9f00-732f-9d67-a087fa117a8a");
    public readonly static Guid MoveCar = new("018fdb6d-8960-7e00-a446-056d1f74e329");
    public readonly static Guid OtherAction = new("018fdb6e-73c0-71c8-a533-0bff3d55eb59");

    // Help received Id's
    public readonly static Guid NoHelp = new("018fdb9c-3a80-7300-8a49-5b3df75adf2a");
    public readonly static Guid NeighboursFamilyHelp = new("018fdb9d-24e0-78ba-9fff-95ac94b38f7c");
    public readonly static Guid WardenVolunteerHelp = new("018fdb9e-0f40-7061-959a-23bfb2bba985");
    public readonly static Guid FirePolice = new("018fdb9e-f9a0-70ba-9475-5793cbf66ece");
    public readonly static Guid EnvironmentAgency = new("018fdb9f-e400-77bf-868a-633a7e27bc8c");
    public readonly static Guid Highways = new("018fdba0-ce60-77e4-a0a6-91ab51596fad");
    public readonly static Guid LocalAuthority = new("018fdba1-b8c0-765c-89f6-bd1d9019db0c");
    public readonly static Guid FloodlineHelp = new("018fdba2-a320-793d-afd9-126986a9a3fb");

    // Warning source Id's
    public readonly static Guid NoWarning = new("018fdbd3-2900-7fed-9c52-9f4668e28618");
    public readonly static Guid FloodlineWarning = new("018fdbd4-1360-7df8-80a2-a8ae26685016");
    public readonly static Guid Television = new("018fdbd4-fdc0-71a4-8973-f5efce80c875");
    public readonly static Guid Radio = new("018fdbd5-e820-7eb6-bee7-d1de8738d312");
    public readonly static Guid SocialMediaInternet = new("018fdbd6-d280-7d17-9fc4-78608036bd36");
    public readonly static Guid WardenVolunteerWarning = new("018fdbd7-bce0-7545-8aa2-a19a5bda2e83");
    public readonly static Guid NeighbourWarning = new("018fdbd8-a740-7dbf-9f3b-393e01305c25");
    public readonly static Guid OtherWarning = new("018fdbd9-91a0-763d-9773-de4670ae0781");

    // Flood warden awareness Id's
    public readonly static Guid BeforeFlooding = new("018fdc0a-1780-793d-9239-fe2a17b52571");
    public readonly static Guid DuringFlooding = new("018fdc0b-01e0-78f7-864b-0297b744acad");
    public readonly static Guid AfterFlooding = new("018fdc0b-ec40-7769-be33-afc5bf808f01");
    public readonly static Guid NotSureFloodWarden = new("018fdc0c-d6a0-73ae-b83c-d13ccfc0da71");
}

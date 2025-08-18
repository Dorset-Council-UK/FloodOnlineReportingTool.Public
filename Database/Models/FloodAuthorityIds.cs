namespace FloodOnlineReportingTool.Database.Models;

/// <summary>
/// The flood authority Id's.
/// Helps ensure consistency and allows easier comparison.
/// </summary>
public static class FloodAuthorityIds
{
    public readonly static Guid EnvironmentAgency = new("018fd118-9400-76d5-a61a-9ff695c06588");
    public readonly static Guid LeadLocalFloodAuthority = new("018fd119-7e60-7384-bb2b-c157b8b576c6");
    public readonly static Guid WaterAuthority = new("018fd11a-68c0-7c64-8412-495da1eeb0ba");
    public readonly static Guid GasBoard = new("018fd11b-5320-7f24-a39d-b76cc8bffc8b");
    public readonly static Guid ElectricityBoard = new("018fd11c-3d80-7348-b528-d2cd55312a98");
    public readonly static Guid CATRespond = new("018fd11d-27e0-76bc-9176-177880b52135");
    public readonly static Guid Voluntary = new("018fd11e-1240-782f-8aa3-f7a3fea26810");
}

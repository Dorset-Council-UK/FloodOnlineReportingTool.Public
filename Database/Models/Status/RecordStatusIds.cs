namespace FloodOnlineReportingTool.Database.Models.Status;

/// <summary>
/// The record status Id's.
/// Helps ensure consistency and allows easier comparison.
/// </summary>
public static class RecordStatusIds
{
    // General Id's
    public readonly static Guid Yes = new("018fead8-6000-7481-985a-c1e3c56a48a0");
    public readonly static Guid No = new("018fead9-4a60-74f3-a824-fe666cd91f99");
    public readonly static Guid NotSure = new("018feada-34c0-7e10-a183-7a5161c397dc");
}

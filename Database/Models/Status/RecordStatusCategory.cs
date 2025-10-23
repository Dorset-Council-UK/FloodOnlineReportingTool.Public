namespace FloodOnlineReportingTool.Database.Models.Status;

/// <summary>
/// The record status categories.
/// Helps ensure consistency and an easier way to get groups of record statuses.
/// </summary>
public static class RecordStatusCategory
{
    public const string FloodReportStatus = "Flood report status";
    public const string Phase = "Area";
    public const string AreaFloodStatus = "Area Flood Status";
    public const string Validation = "Validation";
    public const string Section19 = "Section19 Status";
    public const string DataProtection = "Data Protection";
    public const string General = "General";
}

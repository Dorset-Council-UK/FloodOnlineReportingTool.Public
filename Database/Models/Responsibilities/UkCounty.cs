namespace FloodOnlineReportingTool.Database.Models.Responsibilities;

public record UkCounty()
{
    public string Name { get; init; } = "";
    public string AreaDescription { get; init; } = "";
    public int AdminUnitId { get; init; }
}

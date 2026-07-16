namespace FloodOnlineReportingTool.Database.Models.Imports;

/// <summary>
/// This table is not used by the project but is used to track failed imports for the project.
/// </summary>
public record FailedImports
{
    public Guid Id { get; init; }
    public string Reference { get; init; } = null!;
    public string Reason { get; init; } = null!;
}

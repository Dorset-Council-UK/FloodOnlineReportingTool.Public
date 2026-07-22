namespace FloodOnlineReportingTool.Database.Models.Imports;

/// <summary>
/// This table is not used by the project but is used to track failed imports for the project.
/// </summary>
public record FailedImport
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public string Reference { get; init; } = null!;
    public string Reason { get; init; } = null!;
}

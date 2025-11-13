namespace FloodOnlineReportingTool.Database.Models.Responsibilities;

/// <summary>
///     <para>Information about specific organisations responsible for flood management</para>
///     <para>Includes optional details to display to the user when approproate.</para>
/// </summary>
public record Organisation
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public required string Description { get; init; }
    public string? DataProtectionStatement { get; init; }
    public string? EmergencyPlanning { get; init; }
    public required Guid FloodAuthorityId { get; init; }
    public string? GettingInTouch { get; init; }
    public required Uri Logo { get; init; }
    public required string Name { get; init; }
    public string? SubmissionReply { get; init; }
    public required Uri Website { get; init; }
    public DateTimeOffset LastUpdatedUtc { get; init; } = DateTimeOffset.UtcNow;

    // Navigation properties
    public FloodAuthority FloodAuthority { get; init; } = null!;
}
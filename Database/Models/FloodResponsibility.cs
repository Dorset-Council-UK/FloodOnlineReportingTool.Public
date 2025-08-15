namespace FloodOnlineReportingTool.Database.Models;

public record FloodResponsibility
{
    public required Guid OrganisationId { get; init; }
    public required int AdminUnitId { get; init; }      // admin_unit_id
    public required string Name { get; init; }
    public required string Description { get; init; }   // area_description
    public DateOnly LookupDate { get; init; } = DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime);

    // Navigation properties
    public Organisation Organisation { get; init; } = null!;
}

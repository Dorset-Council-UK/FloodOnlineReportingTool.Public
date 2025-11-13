namespace FloodOnlineReportingTool.Database.Models.Flood;

/// <summary>
/// Represents the broader impacts of a flood, such as health risks, economic losses, etc.
/// </summary>
public record FloodImpact
{
    public FloodImpact(Guid id, string category, string typeName, string? categoryPriority, int optionOrder)
    {
        Id = id;
        Category = category;
        TypeName = typeName;
        CategoryPriority = categoryPriority;
        OptionOrder = optionOrder;
    }

    public Guid Id { get; init; } = Guid.CreateVersion7();
    public string? Category { get; init; }
    public string? TypeName { get; init; }
    public string? TypeDescription { get; init; }
    public string? CategoryPriority { get; init; }
    public int OptionOrder { get; init; }
}
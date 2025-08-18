namespace FloodOnlineReportingTool.Database.Models;

/// <summary>
/// Actions and measures taken to reduce or prevent the impact of flooding
/// </summary>
public record FloodMitigation
{
    public FloodMitigation(Guid id, string category, string typeName, int optionOrder)
    {
        Id = id;
        Category = category;
        TypeName = typeName;
        OptionOrder = optionOrder;
    }

    public Guid Id { get; init; } = Guid.CreateVersion7();
    public string? Category { get; init; }
    public string? TypeName { get; init; }
    public string? TypeDescription { get; init; }
    public int OptionOrder { get; init; }
}

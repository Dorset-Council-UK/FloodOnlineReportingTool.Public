namespace FloodOnlineReportingTool.Database.Models;

/// <summary>
/// Represents specific problems caused by a flood, such as structural damage, water contamination, etc.
/// </summary>
public record FloodProblem
{
    public FloodProblem(Guid id, string category, string? typeName, string typeDescription, int optionOrder)
    {
        Id = id;
        Category = category;
        TypeName = typeName;
        TypeDescription = typeDescription;
        OptionOrder = optionOrder;
    }

    public Guid Id { get; init; } = Guid.CreateVersion7();
    public string? Category { get; init; }
    public string? TypeName { get; init; }
    public string? TypeDescription { get; init; }
    public int OptionOrder { get; init; }
}

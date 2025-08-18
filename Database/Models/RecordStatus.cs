namespace FloodOnlineReportingTool.Database.Models;

public record RecordStatus
{
    public RecordStatus(Guid id, string category, string text, int order)
    {
        Id = id;
        Category = category;
        Text = text;
        Order = order;
    }

    public RecordStatus(Guid id, string category, string text, int order, string description)
    {
        Id = id;
        Category = category;
        Text = text;
        Order = order;
        Description = description;
    }

    public Guid Id { get; init; } = Guid.CreateVersion7();
    public string? Category { get; init; }
    public string? Text { get; init; } // Was TypeName
    public string? Description { get; init; } // Was TypeDescription
    public int Order { get; init; } // Was OptionOrder
}

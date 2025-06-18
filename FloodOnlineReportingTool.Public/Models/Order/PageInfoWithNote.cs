namespace FloodOnlineReportingTool.Public.Models.Order;

internal sealed record PageInfoWithNote : PageInfo
{
    public string? Note { get; }

    public PageInfoWithNote(PageInfo pageInfo) : base(pageInfo)
    {
    }

    public PageInfoWithNote(PageInfo pageInfo, string? note) : base(pageInfo)
    {
        Note = note;
    }
}

using GdsBlazorComponents;

namespace FloodOnlineReportingTool.Public.Models.Order;

internal record PageInfo
{
    public string Url { get; }
    public string Title { get; }

    public PageInfo(ReadOnlySpan<char> url, ReadOnlySpan<char> title)
    {
        // Efficient allocation at construction only
        Url = url.ToString();
        Title = title.ToString();
    }

    public PageInfo(ReadOnlySpan<char> baseUrl, ReadOnlySpan<char> path, ReadOnlySpan<char> title)
    {
        // Efficient allocation at construction only
        Url = string.Concat(baseUrl, path);
        Title = title.ToString();
    }

    public GdsBreadcrumb ToGdsBreadcrumb() => new(Url, Title);
}
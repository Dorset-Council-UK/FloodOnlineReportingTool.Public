using GdsBlazorComponents;

namespace FloodOnlineReportingTool.Public.Models.Order;

internal record PageInfo
{
    public string Url 
        => includeFromSummaryIsTrueInURL
            ? field.Contains("?") ? field + "&fromSummary=true" : field + "?fromSummary=true"
            : field;

    public string Title { get; }

    private bool includeFromSummaryIsTrueInURL { get; }

    public void Deconstruct(out string url, out string title)
    {
        url = Url;
        title = Title;
    }

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

    public PageInfo(PageInfo basePageInfo, bool includeFromSummaryIsTrueInURL)
    {
        Url = basePageInfo.Url;
        Title = basePageInfo.Title;
        this.includeFromSummaryIsTrueInURL = includeFromSummaryIsTrueInURL;
    }

    public GdsBreadcrumb ToGdsBreadcrumb() => new(Url, Title);
}
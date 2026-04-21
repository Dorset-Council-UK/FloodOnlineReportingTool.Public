namespace FloodOnlineReportingTool.Public.Models.Order;

internal static class FloodReportPages
{
    private static ReadOnlySpan<char> BaseUrl => "floodreport";

    public static readonly PageInfo Home = new(BaseUrl, "Report a flood");
    public static readonly PageInfo Overview = new(BaseUrl, "/overview", "View reports");
    public static readonly PageInfo Details = new(BaseUrl, "/overview/details", "View report");
    public static readonly PageInfo Update = new(BaseUrl, "/update", "Update report");
}

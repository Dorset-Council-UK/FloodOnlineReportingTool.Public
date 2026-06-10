namespace FloodOnlineReportingTool.Public.Models.Order;

internal static class MediaPages
{
    private static ReadOnlySpan<char> BaseUrl => "floodreport/media";

    public static readonly PageInfo Home = new(BaseUrl, "/add-photos-or-videos", "Add photos or videos");
}

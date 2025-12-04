namespace FloodOnlineReportingTool.Public.Models.Order;

internal static class SubscriptionPages
{
    private static ReadOnlySpan<char> BaseUrl => "floodreport/contacts/subscribe";

    public static readonly PageInfo Home = new(BaseUrl, "Subscribe for notifications");
    public static readonly PageInfo Verify = new(BaseUrl, "/verify", "Verify your email address");
    public static readonly PageInfo Summary = new(BaseUrl, "/summary", "Confirm your subscription");
}

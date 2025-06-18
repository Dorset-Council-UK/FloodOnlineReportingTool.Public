namespace FloodOnlineReportingTool.Public.Models.Order;

internal static class ContactPages
{
    private static ReadOnlySpan<char> BaseUrl => "floodreport/contacts";

    public static readonly PageInfo Home = new(BaseUrl, "Your contact information");
    public static readonly PageInfo Create = new(BaseUrl, "/create", "New contact information");
    public static readonly PageInfo Change = new(BaseUrl, "/change", "Change contact information");
    public static readonly PageInfo Delete = new(BaseUrl, "/delete", "Delete contact information");
}

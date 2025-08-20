namespace FloodOnlineReportingTool.Public.Models.Order;

internal static class FloodReportCreatePages
{
    private static ReadOnlySpan<char> BaseUrl => "floodreport/create";

    public static readonly PageInfo Home = new(BaseUrl, "Affected property or location");
    public static readonly PageInfo Postcode = new(BaseUrl, "/postcode", "Find address");
    public static readonly PageInfo Address = new(BaseUrl, "/address", "Affected property");
    public static readonly PageInfo PropertyType = new(BaseUrl, "/propertytype?FromLocation=false", "Property type");
    public static readonly PageInfo PropertyTypeFromLocation = new(BaseUrl, "/propertytype?FromLocation=true", "Property type");
    public static readonly PageInfo Confirmation = new(BaseUrl, "/confirmation", "Flood report complete");
    public static readonly PageInfo FloodAreas = new(BaseUrl, "/floodareas", "Flood impact");
    public static readonly PageInfo FloodDuration = new(BaseUrl, "/floodduration", "Flooding duration");
    public static readonly PageInfo FloodSource = new(BaseUrl, "/floodsource", "Source of the flooding");
    public static readonly PageInfo FloodStarted = new(BaseUrl, "/floodstarted", "Flooding started");
    public static readonly PageInfo Location = new(BaseUrl, "/location", "Choose a location");
    public static readonly PageInfo Summary = new(BaseUrl, "/summary", "Check your answers");
    public static readonly PageInfo Vulnerability = new(BaseUrl, "/vulnerability", "Vulnerable persons");
}

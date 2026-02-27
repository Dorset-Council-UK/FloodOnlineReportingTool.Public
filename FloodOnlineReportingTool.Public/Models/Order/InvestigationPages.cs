namespace FloodOnlineReportingTool.Public.Models.Order;

internal static class InvestigationPages
{
    private static ReadOnlySpan<char> BaseUrl => "floodreport/investigation";

    public static readonly PageInfo Home = new(BaseUrl, "Flood investigation");
    public static readonly PageInfo Speed = new(BaseUrl, "/speed", "Flood water details");
    public static readonly PageInfo Destination = new(BaseUrl, "/flooddestination", "Destination");
    public static readonly PageInfo Vehicles = new(BaseUrl, "/vehicles", "Damage to vehicles");
    public static readonly PageInfo InternalHow = new(BaseUrl, "/entry", "Water entry");
    public static readonly PageInfo InternalWhen = new(BaseUrl, "/internal", "Internal flooding");
    public static readonly PageInfo PeakDepth = new(BaseUrl, "/peakdepth", "Peak depth");
    public static readonly PageInfo ServiceImpact = new(BaseUrl, "/serviceimpact", "Impact on services");
    public static readonly PageInfo CommunityImpact = new(BaseUrl, "/communityimpact", "Impact on the community");
    public static readonly PageInfo Blockages = new(BaseUrl, "/blockages", "Blockages");
    public static readonly PageInfo ActionsTaken = new(BaseUrl, "/actionstaken", "Actions taken");
    public static readonly PageInfo HelpReceived = new(BaseUrl, "/helpreceived", "Help received");
    public static readonly PageInfo Warnings = new(BaseUrl, "/warnings", "Before the flooding");
    public static readonly PageInfo WarningSources = new(BaseUrl, "/warningsources", "Warning sources");
    public static readonly PageInfo Floodline = new(BaseUrl, "/floodline", "Floodline warning");
    public static readonly PageInfo History = new(BaseUrl, "/history", "Flood history");
    public static readonly PageInfo Summary = new(BaseUrl, "/summary", "Check your answers");
    public static readonly PageInfo Confirmation = new(BaseUrl, "/confirmation", "Investigation complete");
    public static readonly PageInfo FirstPage = Speed;
}
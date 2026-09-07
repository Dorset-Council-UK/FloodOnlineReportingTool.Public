namespace FloodOnlineReportingTool.Public.Models.Order;

internal static class FloodReportCreatePages
{
    private static ReadOnlySpan<char> BaseUrl => "floodreport/create";

    public static readonly PageInfo Home = new(BaseUrl, "Affected property or location");
    public static readonly PageInfo HomeWithFromSummaryIsTrue = new(Home, true);
    public static readonly PageInfo Postcode = new(BaseUrl, "/postcode", "Find postcode");
    public static readonly PageInfo PostcodeWithFromSummaryIsTrue = new(Postcode, true);
    public static readonly PageInfo Address = new(BaseUrl, "/address", "Affected property");
    public static readonly PageInfo AddressWithFromSummaryIsTrue = new(Address, true);
    public static readonly PageInfo PropertyType = new(BaseUrl, "/propertytype", "Property type");
    public static readonly PageInfo PropertyTypeWithFromSummaryIsTrue = new(PropertyType, true);
    public static readonly PageInfo Confirmation = new(BaseUrl, "/confirmation", "Stage 1 complete");
    public static readonly PageInfo ConfirmationWithFromSummaryIsTrue = new(Confirmation, true);
    public static readonly PageInfo FloodAreas = new(BaseUrl, "/floodareas", "Flood impact");
    public static readonly PageInfo FloodAreasWithFromSummaryIsTrue = new(FloodAreas, true);
    public static readonly PageInfo FloodDuration = new(BaseUrl, "/floodduration", "Flooding duration");
    public static readonly PageInfo FloodDurationWithFromSummaryIsTrue = new(FloodDuration, true);
    public static readonly PageInfo TemporaryPostcode = new(BaseUrl, "/temporarypostcode", "Find temporary address");
    public static readonly PageInfo TemporaryPostcodeWithFromSummaryIsTrue = new(TemporaryPostcode, true);
    public static readonly PageInfo TemporaryAddress = new(BaseUrl, "/temporaryaddress", "Temporary address");
    public static readonly PageInfo TemporaryAddressWithFromSummaryIsTrue = new(TemporaryAddress, true);
    public static readonly PageInfo Cause = new(BaseUrl, "/cause", "Cause");
    public static readonly PageInfo CauseWithFromSummaryIsTrue = new(Cause, true);
    public static readonly PageInfo SecondaryCause = new(BaseUrl, "/secondarycause", "Secondary cause");
    public static readonly PageInfo SecondaryCauseWithFromSummaryIsTrue = new(SecondaryCause, true);
    public static readonly PageInfo FloodStarted = new(BaseUrl, "/floodstarted", "Flooding started");
    public static readonly PageInfo FloodStartedWithFromSummaryIsTrue = new(FloodStarted, true);
    public static readonly PageInfo Location = new(BaseUrl, "/location", "Choose a location");
    public static readonly PageInfo LocationWithFromSummaryIsTrue = new(Location, true);
    public static readonly PageInfo Summary = new(BaseUrl, "/summary", "Check your answers");
    public static readonly PageInfo SummaryWithFromSummaryIsTrue = new(Summary, true);
    public static readonly PageInfo Vulnerability = new(BaseUrl, "/vulnerability", "Vulnerable persons");
    public static readonly PageInfo VulnerabilityWithFromSummaryIsTrue = new(Vulnerability, true);
}

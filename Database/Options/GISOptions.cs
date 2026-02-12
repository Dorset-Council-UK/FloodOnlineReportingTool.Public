namespace FloodOnlineReportingTool.Database.Options;

public record GISOptions
{
    public const string SectionName = "GIS";

    public required string ApiKey { get; init; }
    public required string PathBase { get; init; }
    public required Uri AddressSearchUrl { get; init; }
    public required Uri NearestAddressesUrl { get; init; }
    public required int AccessTokenIssueDurationMonths { get; init; } = 6;
    public required string OSApiKey { get; init; }
    public string? OSLicenceNumber { get; init; } = string.Empty;
    public required int DataRetentionYears { get; init; } = 7;
}

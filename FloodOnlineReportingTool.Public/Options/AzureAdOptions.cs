namespace FloodOnlineReportingTool.Public.Options
{
    public class AzureAdOptions
    {
        public const string SectionName = "AzureAd";

        public required string Instance { get; set; }
        public required string Domain { get; set; }
        public required string TenantId { get; set; }
        public required string ClientId { get; set; }
        public string? ClientSecret { get; set; }
        public string? ClientCertificateThumbprint { get; set; }
        public string? CallbackPath { get; set; }
        public string? ResponseType { get; set; }
        public string[]? Scopes { get; set; }
        public bool ValidateIssuer { get; set; } = true;
    }

}

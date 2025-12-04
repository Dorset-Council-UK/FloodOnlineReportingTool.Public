namespace FloodOnlineReportingTool.Public.Models.FloodReport.Contact.Subscribe;

public class VerifyModel
{
    public Guid Id { get; init; }
    public string ContactName { get; set; } = "";
    public string EmailAddress { get; set; } = "";
    public bool IsEmailVerified { get; set; } = false;
    public DateTimeOffset VerificationExpiryUtc { get; init; }


    public int? EnteredCodeNumber { get; set; }
    public string? EnteredCodeText { get; set; }
}

namespace FloodOnlineReportingTool.Public.Models.FloodReport.Contact.Subscribe;

public class VerifyModel: SubscribeModel
{
    public Guid Id { get; init; }
    public DateTimeOffset VerificationExpiryUtc { get; init; }


    public int? EnteredCodeNumber { get; set; }
    public string? EnteredCodeText { get; set; }
}

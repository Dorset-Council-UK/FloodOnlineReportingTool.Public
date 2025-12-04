using FloodOnlineReportingTool.Database.Models.Contact;

namespace FloodOnlineReportingTool.Public.Models.FloodReport.Contact.Subscribe;

public class SubscribeModel
{
    public string ContactName { get; set; } = "";
    public string EmailAddress { get; set; } = "";
    public bool IsEmailVerified { get; set; } = false;
    public bool IsSubscribed { get; set; } = false;
    public DateTimeOffset CreatedUtc { get; init; }
    public DateTimeOffset RedactionDate { get; init; }

    public Guid? ContactRecordId { get; set; }
    public ContactRecord? ContactRecord { get; set; }
}

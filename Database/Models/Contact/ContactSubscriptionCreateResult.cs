namespace FloodOnlineReportingTool.Database.Models.Contact;

public record ContactSubscriptionCreateResult(bool IsSuccess, ContactSubscriptionRecord? ContactSubscriptionRecord, ICollection<string> Errors)
{
    internal static ContactSubscriptionCreateResult Success(ContactSubscriptionRecord ContactSubscriptionRecord) => new(IsSuccess: true, ContactSubscriptionRecord, []);

    internal static ContactSubscriptionCreateResult Failure(ICollection<string> errors) => new(IsSuccess: false, ContactSubscriptionRecord: null, errors);
}

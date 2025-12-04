namespace FloodOnlineReportingTool.Database.Models.Contact.Subscribe;

public record SubscribeCreateOrUpdateResult(bool IsSuccess, SubscribeRecord? ContactSubscriptionRecord, ICollection<string> Errors)
{
    internal static SubscribeCreateOrUpdateResult Success(SubscribeRecord ContactSubscriptionRecord) => new(IsSuccess: true, ContactSubscriptionRecord, []);

    internal static SubscribeCreateOrUpdateResult Failure(ICollection<string> errors) => new(IsSuccess: false, ContactSubscriptionRecord: null, errors);
}

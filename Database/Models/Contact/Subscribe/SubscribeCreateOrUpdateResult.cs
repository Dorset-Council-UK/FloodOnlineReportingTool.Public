namespace FloodOnlineReportingTool.Database.Models.Contact.Subscribe;

public record SubscribeCreateOrUpdateResult(bool IsSuccess, SubscribeRecord? SubscriptionRecord, ICollection<string> Errors)
{
    internal static SubscribeCreateOrUpdateResult Success(SubscribeRecord SubscriptionRecord) => new(IsSuccess: true, SubscriptionRecord, []);

    internal static SubscribeCreateOrUpdateResult Failure(ICollection<string> errors) => new(IsSuccess: false, SubscriptionRecord: null, errors);
}

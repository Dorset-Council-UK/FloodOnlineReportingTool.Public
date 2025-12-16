namespace FloodOnlineReportingTool.Database.Models.Flood;

// Keeping separate from FloodReportCreateOrUpdateResult as we may want to add something about which emails were sent later?
// Currently just a duplicate of FloodReportCreateOrUpdateResult though
public record SubscriptionResult(bool IsSuccess, FloodReport? FloodReport, ICollection<string> Errors)
{
    internal static SubscriptionResult Success(FloodReport floodReport) => new(IsSuccess: true, floodReport, []);

    internal static SubscriptionResult Failure(ICollection<string> errors) => new(IsSuccess: false, FloodReport: null, errors);
}
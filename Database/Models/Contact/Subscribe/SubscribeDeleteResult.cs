namespace FloodOnlineReportingTool.Database.Models.Contact.Subscribe;

public record SubscribeDeleteResult(bool IsSuccess, ICollection<string> Errors)
{
    internal static SubscribeDeleteResult Success() => new(IsSuccess: true, []);

    internal static SubscribeDeleteResult Failure(ICollection<string> errors) => new(IsSuccess: false, errors);
}

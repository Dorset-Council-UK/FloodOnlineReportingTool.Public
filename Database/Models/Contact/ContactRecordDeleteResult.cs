namespace FloodOnlineReportingTool.Database.Models.Contact;

public record ContactRecordDeleteResult(bool IsSuccess, ICollection<string> Errors)
{
    internal static ContactRecordDeleteResult Success() => new(IsSuccess: true, []);

    internal static ContactRecordDeleteResult Failure(ICollection<string> errors) => new(IsSuccess: false, errors);
}

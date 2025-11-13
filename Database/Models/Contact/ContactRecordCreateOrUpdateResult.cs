namespace FloodOnlineReportingTool.Database.Models.Contact;

public record ContactRecordCreateOrUpdateResult(bool IsSuccess, ContactRecord? ContactRecord, ICollection<string> Errors)
{
    internal static ContactRecordCreateOrUpdateResult Success(ContactRecord contactRecord) => new(IsSuccess: true, contactRecord, []);

    internal static ContactRecordCreateOrUpdateResult Failure(ICollection<string> errors) => new(IsSuccess: false, ContactRecord: null, errors);
}

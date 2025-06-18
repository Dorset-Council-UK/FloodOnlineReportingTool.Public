using FloodOnlineReportingTool.Public.Models.FloodReport.Contact;

namespace FloodOnlineReportingTool.DataAccess.Models;

internal static class ContactRecordExtensions
{
    internal static ContactModel ToContactModel(this ContactRecord contactRecord)
    {
        return new()
        {
            Id = contactRecord.Id,
            ContactType = contactRecord.ContactType,
            ContactName = contactRecord.ContactName,
            EmailAddress = contactRecord.EmailAddress,
            PhoneNumber = contactRecord.PhoneNumber,
        };
    }
}

using FloodOnlineReportingTool.Public.Models.FloodReport.Contact;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace FloodOnlineReportingTool.Database.Models;
#pragma warning restore IDE0130 // Namespace does not match folder structure

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

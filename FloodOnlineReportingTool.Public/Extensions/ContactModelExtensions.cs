using FloodOnlineReportingTool.Database.Models;

namespace FloodOnlineReportingTool.Public.Models.FloodReport.Contact;

internal static class ContactModelExtensions
{
    internal static ContactRecordDto ToDto(this ContactModel contactModel)
    {
        return new()
        {
            ContactType = contactModel.ContactType ?? ContactRecordType.Unknown,
            ContactName = contactModel.ContactName ?? "",
            EmailAddress = contactModel.EmailAddress ?? "",
            PhoneNumber = contactModel.PhoneNumber,
        };
    }
}

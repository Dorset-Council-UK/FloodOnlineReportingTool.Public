using FloodOnlineReportingTool.Contracts.Shared;
using FloodOnlineReportingTool.Database.Models.Contact;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace FloodOnlineReportingTool.Public.Models.FloodReport.Contact;
#pragma warning restore IDE0130 // Namespace does not match folder structure

internal static class ContactModelExtensions
{
    internal static ContactRecordDto ToDto(this ContactModel contactModel)
    {
        return new()
        {
            UserId = contactModel.ContactUserId,
            ContactType = contactModel.ContactType ?? ContactRecordType.Unknown,
            ContactName = contactModel.ContactName ?? "",
            EmailAddress = contactModel.EmailAddress ?? "",
            PhoneNumber = contactModel.PhoneNumber,
            IsRecordOwner = contactModel.IsRecordOwner
        };
    }
}

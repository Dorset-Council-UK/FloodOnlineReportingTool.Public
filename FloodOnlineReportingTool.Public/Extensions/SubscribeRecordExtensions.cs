using FloodOnlineReportingTool.Database.Models.Contact.Subscribe;
using FloodOnlineReportingTool.Public.Models.FloodReport.Contact;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace FloodOnlineReportingTool.Database.Models;
#pragma warning restore IDE0130 // Namespace does not match folder structure

internal static class SubscribeRecordExtensions
{
    internal static ContactModel ToContactModel(this SubscribeRecord subscribeRecord)
    {
        return new()
        {
            EmailAddress = subscribeRecord.EmailAddress,
            ContactName = subscribeRecord.ContactName,
            ContactType = subscribeRecord.ContactType,
            Id = subscribeRecord.Id,
            IsEmailVerified = subscribeRecord.IsEmailVerified,
            IsRecordOwner = subscribeRecord.IsRecordOwner,
            IsSubscribed = subscribeRecord.IsSubscribed,
            PhoneNumber = subscribeRecord.PhoneNumber,
            ContactUserId = subscribeRecord.ContactRecordId
        };
    }
}

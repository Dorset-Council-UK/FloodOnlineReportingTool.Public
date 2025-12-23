using FloodOnlineReportingTool.Database.Models.Contact;
using FloodOnlineReportingTool.Public.Models.FloodReport.Contact;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace FloodOnlineReportingTool.Database.Models;
#pragma warning restore IDE0130 // Namespace does not match folder structure

internal static class ContactRecordExtensions
{
    internal static ContactModel ToContactModel(this ContactRecord contactRecord)
    {
        if (contactRecord.SubscribeRecords.FirstOrDefault() is not Contact.Subscribe.SubscribeRecord subscriptionRecord)
        {
            return new()
            {
                Id = contactRecord.Id,
                ContactUserId = contactRecord.ContactUserId
            };
        }
        return new()
        {
            Id = contactRecord.Id,
            ContactName = subscriptionRecord.ContactName,
            ContactType = subscriptionRecord.ContactType,
            EmailAddress = subscriptionRecord.EmailAddress,
            IsRecordOwner = subscriptionRecord.IsRecordOwner,
            IsEmailVerified = subscriptionRecord.IsEmailVerified,
            IsSubscribed = subscriptionRecord.IsSubscribed,
            PhoneNumber = subscriptionRecord.PhoneNumber,
            ContactUserId = contactRecord.ContactUserId

        };
    }
}

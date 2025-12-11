using FloodOnlineReportingTool.Database.Models.Contact;
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
            ContactName = contactRecord.SubscribeRecords.FirstOrDefault()?.ContactName,
            ContactType = contactRecord.SubscribeRecords.FirstOrDefault()?.ContactType,
            EmailAddress = contactRecord.SubscribeRecords.FirstOrDefault()?.EmailAddress,
            IsRecordOwner = contactRecord.SubscribeRecords.FirstOrDefault()?.IsRecordOwner ?? false,
            IsEmailVerified = contactRecord.SubscribeRecords.FirstOrDefault()?.IsEmailVerified ?? false,
            IsSubscribed = contactRecord.SubscribeRecords.FirstOrDefault()?.IsSubscribed ?? false,
            PhoneNumber = contactRecord.SubscribeRecords.FirstOrDefault()?.PhoneNumber,
            ContactUserId = contactRecord.ContactUserId

        };
    }
}

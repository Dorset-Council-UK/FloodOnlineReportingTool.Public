using FloodOnlineReportingTool.Contracts;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace FloodOnlineReportingTool.Database.Models;
#pragma warning restore IDE0130 // Namespace does not match folder structure

internal static class ContactRecordExtensions
{
    internal static ContactRecordCreated ToMessageCreated(this ContactRecord contactRecord, string floodReportReference)
    {
        return new ContactRecordCreated(
            floodReportReference,
            contactRecord.Id,
            contactRecord.ContactType.ToString(),
            contactRecord.CreatedUtc
        );
    }

    internal static ContactRecordUpdated ToMessageUpdated(this ContactRecord contactRecord, string floodReportReference)
    {
        return new ContactRecordUpdated(
            floodReportReference,
            contactRecord.Id,
            contactRecord.ContactType.ToString(),
            contactRecord.UpdatedUtc ?? DateTimeOffset.UtcNow
        );
    }

    internal static ContactRecordDeleted ToMessageDeleted(this ContactRecord contactRecord, string floodReportReference)
    {
        return new ContactRecordDeleted(
            floodReportReference,
            contactRecord.Id,
            contactRecord.ContactType.ToString(),
            DateTimeOffset.UtcNow
        );
    }
}

namespace FloodOnlineReportingTool.Database.Models.Contact.Subscribe;

public static class PersonalDataExtensions
{
    public static IQueryable<SubscribeRecord> FilterPersonalData(
        this IQueryable<SubscribeRecord> query,
        bool includePersonalData)
    {
        if (includePersonalData)
        {
            return query;
        }

        return query.Select(sr => new SubscribeRecord
        {
            Id = sr.Id,
            IsRecordOwner = sr.IsRecordOwner,
            ContactType = sr.ContactType,
            IsEmailVerified = sr.IsEmailVerified,
            IsSubscribed = sr.IsSubscribed,
            CreatedUtc = sr.CreatedUtc,
            RedactionDate = sr.RedactionDate,
            VerificationExpiryUtc = sr.VerificationExpiryUtc,
            ContactRecordId = sr.ContactRecordId,
            ContactRecord = sr.ContactRecord
        });
    }

    public static IEnumerable<SubscribeRecord> FilterPersonalData(
        this IEnumerable<SubscribeRecord> collection,
        bool includePersonalData)
    {
        if (includePersonalData)
        {
            return collection;
        }

        return collection.Select(sr => sr with
        {
            ContactName = string.Empty,
            EmailAddress = string.Empty,
            PhoneNumber = null,
            VerificationCode = null
        });
    }
}

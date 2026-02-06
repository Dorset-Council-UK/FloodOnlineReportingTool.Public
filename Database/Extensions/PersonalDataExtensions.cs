using FloodOnlineReportingTool.Database.Compliance;

namespace FloodOnlineReportingTool.Database.Models.Contact.Subscribe;

public static class PersonalDataExtensions
{
    public static IQueryable<SubscribeRecord> RoleBasedFilterPersonalData(
        this IQueryable<SubscribeRecord> query,
        bool hasPersonalDataRole)
    {
        if (hasPersonalDataRole)
        {
            return query;
        }

        return FilterPersonalData(query);
    }

    public static IQueryable<SubscribeRecord> FilterPersonalData(
        this IQueryable<SubscribeRecord> query)
    {
        return query.Select(static sr => new SubscribeRecord
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
            ContactRecord = sr.ContactRecord,
            ContactName = StarRedactor.StarConverter(sr.ContactName, true),
            EmailAddress = StarRedactor.StarConverter(sr.EmailAddress, true),
            PhoneNumber = StarRedactor.StarConverter(sr.PhoneNumber ?? ""),
            VerificationCode = null
        });
    }

    public static IEnumerable<SubscribeRecord> RoleBasedFilterPersonalData(
        this IEnumerable<SubscribeRecord> collection,
        bool hasPersonalDataRole)
    {
        if (hasPersonalDataRole)
        {
            return collection;
        }

        return FilterPersonalData(collection);
    }

    public static IEnumerable<SubscribeRecord> FilterPersonalData(
        this IEnumerable<SubscribeRecord> collection)
    {
        return collection.Select(sr => sr with
        {
            ContactName = StarRedactor.StarConverter(sr.ContactName, true),
            EmailAddress = StarRedactor.StarConverter(sr.EmailAddress, true),
            PhoneNumber = StarRedactor.StarConverter(sr.PhoneNumber ?? ""),
            VerificationCode = null
        });
    }
}

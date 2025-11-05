using FloodOnlineReportingTool.Contracts.Shared;
using FloodOnlineReportingTool.Database.DbContexts;
using FloodOnlineReportingTool.Database.Models.Contact;
using FloodOnlineReportingTool.Database.Models.Flood;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace FloodOnlineReportingTool.Database.Repositories;

public class ContactRecordRepository(PublicDbContext context, IPublishEndpoint publishEndpoint) : IContactRecordRepository
{
    public async Task<ContactRecord?> GetContactById(Guid contactRecordId, CancellationToken ct)
    {
        return await context.ContactRecords
            .Where(cr => cr.Id == contactRecordId)
            .Include(cr => cr.FloodReports)
            .FirstOrDefaultAsync(ct)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyCollection<ContactRecord>> GetContactsByReport(Guid floodReportId, CancellationToken ct)
    {

        return await context.FloodReports
            .Where(fr => fr.Id == floodReportId)
            .SelectMany(fr => fr.ContactRecords)
            .ToListAsync(ct)
            .ConfigureAwait(false);
    }

    public async Task<ContactRecord> CreateForReport(Guid floodReportId, ContactRecordDto dto, CancellationToken ct)
    {
        var floodReport = await context.FloodReports
            .Include(o => o.ContactRecords)
            .Where(o => o.Id == floodReportId)
            .FirstOrDefaultAsync(ct)
            .ConfigureAwait(false);

        if (floodReport == null)
        {
            throw new InvalidOperationException($"No flood report found");
        }

        // Only allow 1 instance of each type of contact to be created
        var contactCount = floodReport.ContactRecords.Count;
        if (floodReport.ContactRecords.Any(o => o.ContactType == dto.ContactType))
        {
            throw new InvalidOperationException($"A contact record of type {dto.ContactType} already exists for the user");
        }

        // Create the contact record.
        ContactRecord? contactRecord = null;
        var now = DateTimeOffset.UtcNow;
        if (dto.UserId != null)
        {
            // Does the user already have a contact record?
            contactRecord = await context.ContactRecords
                .Where(o => o.ContactUserId == dto.UserId)
                .FirstOrDefaultAsync(ct)
                .ConfigureAwait(false);
        }

        if (contactRecord == null)
        {
            contactRecord = new ContactRecord
            {
                CreatedUtc = now,
                RedactionDate = now.AddMonths(6), // This is the default redaction period

                ContactUserId = dto.UserId,
                ContactType = dto.ContactType,
                ContactName = dto.ContactName,
                EmailAddress = dto.EmailAddress,
                PhoneNumber = dto.PhoneNumber,
                FloodReports = new List<FloodReport> { floodReport },
            };
            context.ContactRecords.Add(contactRecord);

            if (contactCount == 0)
            {
                // The first contact added is marked as the report owner
                floodReport.ReportOwnerId = contactRecord.Id;
            }
            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        if (contactRecord?.Id == null)
        {
            throw new InvalidOperationException($"Contact record was not correctly created.");
        }

        // Do we need to add this contact record to the flood report?
        var existingLink = floodReport.ContactRecords.Contains(contactRecord);
        if (existingLink != true)
        //{
        //    throw new InvalidOperationException($"FloodReport already has a contact of type {dto.ContactType}.");
        //}
        //else
        {
            // Link the contact to the flood report
            floodReport.ContactRecords.Add(contactRecord);
            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        // This system is fully responsible for all contact communication. No notifications are sent out at this point.
        // Add the contact record to the flood report and the message to the database
        await context
            .SaveChangesAsync(ct)
            .ConfigureAwait(false);

        return contactRecord;
    }

    public async Task<ContactRecord> UpdateForUser(Guid userId, Guid id, ContactRecordDto dto, CancellationToken ct)
    {

        var contactRecord = await context.ContactRecords
            .Where(fc => fc.ContactUserId == userId &&
                        fc.Id == id &&
                        fc.ContactType == dto.ContactType)
            .Include(fc => fc.FloodReports)
            .FirstOrDefaultAsync(ct)
            .ConfigureAwait(false);

        if (contactRecord == null)
        {
            throw new InvalidOperationException("No contact record found for this record type");
        }

        // Update the fields we choose
        bool emailNotChanged = string.Equals(contactRecord.EmailAddress, dto.EmailAddress, StringComparison.OrdinalIgnoreCase);
        contactRecord = contactRecord with
        {
            UpdatedUtc = DateTimeOffset.UtcNow,

            ContactUserId = userId,
            ContactType = dto.ContactType,
            ContactName = dto.ContactName,
            EmailAddress = dto.EmailAddress,
            PhoneNumber = dto.PhoneNumber,
        };

        if (!emailNotChanged)
        {
            contactRecord = contactRecord with
            {
                IsEmailVerified = false,
            };
        }

        await context.SaveChangesAsync(ct).ConfigureAwait(false);

        // This system is fully responsible for all contact communication. No notifications are sent out at this point.
        // Update the contact record and add the message to the database
        await context
            .SaveChangesAsync(ct)
            .ConfigureAwait(false);

        return contactRecord;
    }

    public async Task DeleteById(Guid contactRecordId, ContactRecordType contactType, CancellationToken ct)
    {
        var contactRecord = await context.ContactRecords
            .Where(fc => fc.Id == contactRecordId &&
                        fc.ContactType == contactType)
            .Include(fc => fc.FloodReports)
            .FirstOrDefaultAsync(ct)
            .ConfigureAwait(false);

        if (contactRecord == null)
        {
            throw new InvalidOperationException("No contact record found for this record type");
        }

        // Remove the contact record from the flood report
        context.ContactRecords.Remove(contactRecord);

        // This system is fully responsible for all contact communication. No notifications are sent out at this point.
        // Remove the contact record and add the message to the database
        await context
            .SaveChangesAsync(ct)
            .ConfigureAwait(false);

        return;
    }

    public async Task<IList<ContactRecordType>> GetUnusedRecordTypes(Guid floodReportId, CancellationToken ct)
    {
        var usedRecordTypes = await context.FloodReports
            .AsNoTracking()
            .Where(o => o.Id == floodReportId)
            .SelectMany(o => o.ContactRecords)
            .Select(o => o.ContactType)
            .ToListAsync(ct)
            .ConfigureAwait(false);

        return [.. Enum.GetValues<ContactRecordType>().Where(o => o != ContactRecordType.Unknown && !usedRecordTypes.Contains(o))];
    }

    public async Task<int> CountUnusedRecordTypes(Guid floodReportId, CancellationToken ct)
    {
        // get how many contact record types are in the enum
        var allRecordTypes = Enum.GetValues<ContactRecordType>()
            .Count(o => o != ContactRecordType.Unknown);

        // get how many unique contact record types are in the database
        var usedRecordTypes = await context.FloodReports
            .AsNoTracking()
            .Where(o => o.Id == floodReportId)
            .SelectMany(o => o.ContactRecords)
            .Select(o => o.ContactType)
            .Distinct()
            .CountAsync(ct)
            .ConfigureAwait(false);

        // return the difference
        return allRecordTypes - usedRecordTypes;
    }
}

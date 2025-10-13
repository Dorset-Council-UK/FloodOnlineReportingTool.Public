using FloodOnlineReportingTool.Database.DbContexts;
using FloodOnlineReportingTool.Database.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using static MassTransit.ValidationResultExtensions;

namespace FloodOnlineReportingTool.Database.Repositories;

public class ContactRecordRepository(PublicDbContext context, IPublishEndpoint publishEndpoint) : IContactRecordRepository
{
    public async Task<ContactRecord?> ReportedByUser(Guid userId, Guid id, CancellationToken ct)
    {

        return await context.FloodReportContacts
            .Where(fc => fc.FloodReport.ReportedByUserId == userId && fc.ContactRecordId == id)
            .Select(fc => fc.ContactRecord)
            .FirstOrDefaultAsync(ct)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyCollection<ContactRecord>> AllReportedByUser(Guid userId, CancellationToken ct)
    {

        return await context.FloodReportContacts
            .Where(fc => fc.FloodReport.ReportedByUserId == userId)
            .Select(fc => fc.ContactRecord)
            .OrderByDescending(cr => cr.CreatedUtc)
            .ToListAsync(ct)
            .ConfigureAwait(false);
    }

    public async Task<ContactRecord> CreateForReport(Guid floodReportId, ContactRecordDto dto, CancellationToken ct)
    {
        var floodReport = await context.FloodReports
            .AsNoTracking()
            .Include(o => o.ContactRecords)
            .Where(o => o.Id == floodReportId)
            .FirstOrDefaultAsync(ct)
            .ConfigureAwait(false);

        if (floodReport == null)
        {
            throw new InvalidOperationException($"No flood report found");
        }

        // Only allow 1 instance of each type of contact to be created
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
                .Include(o => o.FloodReports)
                .AsNoTracking()
                .Where(o => o.UserId == dto.UserId && o.ContactType == dto.ContactType)
                .FirstOrDefaultAsync(ct)
                .ConfigureAwait(false);

            if(contactRecord == null)
            {
                contactRecord = new ContactRecord
                {
                    CreatedUtc = now,
                    RedactionDate = now.AddMonths(6), // This is the default redaction period

                    UserId = dto.UserId,
                    ContactType = dto.ContactType,
                    ContactName = dto.ContactName,
                    EmailAddress = dto.EmailAddress,
                    PhoneNumber = dto.PhoneNumber
                };

                context.ContactRecords.Add(contactRecord);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        // Do we need to add this contact record to the flood report?
        var existingLink = await context.FloodReportContacts
                .FirstOrDefaultAsync(link =>
                    link.FloodReportId == floodReportId &&
                    link.ContactType == dto.ContactType)
                .ConfigureAwait(false);

        if (existingLink != null)
        {
            throw new InvalidOperationException($"FloodReport already has a contact of type {dto.ContactType}.");
        }

        if (contactRecord?.Id == null)
        {
            throw new InvalidOperationException($"Contact record was not correctly created.");
        }

        // Link the contact to the flood report
        var link = new FloodReportContact
        {
            FloodReportId = floodReportId,
            ContactRecordId = contactRecord.Id,
            ContactType = dto.ContactType
        };

        context.FloodReportContacts.Add(link);
        await context.SaveChangesAsync().ConfigureAwait(false);

        // Publish a created message to the message system
        var message = contactRecord.ToMessageCreated(floodReport.Reference);
        await publishEndpoint
            .Publish(message, ct)
            .ConfigureAwait(false);

        // Add the contact record to the flood report and the message to the database
        await context
            .SaveChangesAsync(ct)
            .ConfigureAwait(false);

        return contactRecord;
    }

    public async Task<ContactRecord> UpdateForUser(Guid userId, Guid id, ContactRecordDto dto, CancellationToken ct)
    {

        var contactFloodRecord = await context.FloodReportContacts
            .Where(fc => fc.FloodReport.ReportedByUserId == userId && 
                        fc.ContactRecordId == id && 
                        fc.ContactType == dto.ContactType)
            .Select(fc => new
            {
                ContactRecord = fc.ContactRecord,
                FloodReport = fc.FloodReport
            })
            .FirstOrDefaultAsync(ct)
            .ConfigureAwait(false);

        if (contactFloodRecord == null)
        {
            throw new InvalidOperationException("No contact record found for this record type");
        }

        var contactRecord = contactFloodRecord.ContactRecord;
        var floodReport = contactFloodRecord.FloodReport;

        // Update the fields we choose
        contactRecord = contactRecord with
        {
            UpdatedUtc = DateTimeOffset.UtcNow,

            UserId = userId,
            ContactType = dto.ContactType,
            ContactName = dto.ContactName,
            EmailAddress = dto.EmailAddress,
            PhoneNumber = dto.PhoneNumber,
        };
        await context.SaveChangesAsync(ct).ConfigureAwait(false);

        // Publish a created message to the message system
        var message = contactRecord.ToMessageUpdated(floodReport.Reference);
        await publishEndpoint
            .Publish(message, ct)
            .ConfigureAwait(false);

        // Update the contact record and add the message to the database
        await context
            .SaveChangesAsync(ct)
            .ConfigureAwait(false);

        return contactRecord;
    }

    public async Task DeleteForUser(Guid userId, Guid id, ContactRecordType contactType, CancellationToken ct)
    {
        var contactFloodRecord = await context.FloodReportContacts
            .Where(fc => fc.FloodReport.ReportedByUserId == userId &&
                        fc.ContactRecordId == id &&
                        fc.ContactType == contactType)
            .Select(fc => new
            {
                ContactRecord = fc.ContactRecord,
                FloodReport = fc.FloodReport
            })
            .FirstOrDefaultAsync(ct)
            .ConfigureAwait(false);

        if (contactFloodRecord == null)
        {
            throw new InvalidOperationException("No contact record found for this record type");
        }

        var contactRecord = contactFloodRecord.ContactRecord;
        var floodReport = contactFloodRecord.FloodReport;

        // Remove the contact record from the flood report
        context.ContactRecords.Remove(contactRecord);

        // Publish a deleted message to the message system
        var message = contactRecord.ToMessageDeleted(floodReport.Reference);
        await publishEndpoint
            .Publish(message, ct)
            .ConfigureAwait(false);

        // Remove the contact record and add the message to the database
        await context
            .SaveChangesAsync(ct)
            .ConfigureAwait(false);

        return;
    }

    public async Task<IList<ContactRecordType>> GetUnusedRecordTypes(Guid userId, CancellationToken ct)
    {
        var usedRecordTypes = await context.FloodReports
            .AsNoTracking()
            .Include(o => o.ContactRecords)
            .Where(o => o.ReportedByUserId == userId)
            .SelectMany(o => o.ContactRecords)
            .Select(o => o.ContactType)
            .ToListAsync(ct)
            .ConfigureAwait(false);

        return [..Enum.GetValues<ContactRecordType>().Where(o => o != ContactRecordType.Unknown && !usedRecordTypes.Contains(o))];
    }

    public async Task<int> CountUnusedRecordTypes(Guid userId, CancellationToken ct)
    {
        // get how many contact record types are in the enum
        var allRecordTypes = Enum.GetValues<ContactRecordType>()
            .Count(o => o != ContactRecordType.Unknown);

        // get how many unique contact record types are in the database
        var usedRecordTypes = await context.FloodReports
            .AsNoTracking()
            .Include(o => o.ContactRecords)
            .Where(o => o.ReportedByUserId == userId)
            .SelectMany(o => o.ContactRecords)
            .Select(o => o.ContactType)
            .Distinct()
            .CountAsync(ct)
            .ConfigureAwait(false);

        // return the difference
        return allRecordTypes - usedRecordTypes;
    }
}

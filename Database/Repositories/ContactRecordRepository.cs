using FloodOnlineReportingTool.Database.DbContexts;
using FloodOnlineReportingTool.Database.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using static MassTransit.ValidationResultExtensions;

namespace FloodOnlineReportingTool.Database.Repositories;

public class ContactRecordRepository(PublicDbContext context, IPublishEndpoint publishEndpoint) : IContactRecordRepository
{
    public async Task<FloodReport?> ReportedByUser(Guid contactUserId, Guid floodReportId, CancellationToken ct)
    {

        return await context.FloodReports
            .Where(fr => fr.ReportOwner != null &&
                 fr.ReportOwner.ContactUserId == contactUserId &&
                 fr.Id == floodReportId)
            .FirstOrDefaultAsync(ct)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyCollection<FloodReport>> AllReportedByUser(Guid contactUserId, CancellationToken ct)
    {

        return await context.FloodReports
            .Where(fc => fc.ReportOwner != null &&
                 fc.ReportOwner.ContactUserId == contactUserId)
            .OrderByDescending(cr => cr.CreatedUtc)
            .ToListAsync(ct)
            .ConfigureAwait(false);
    }

    public async Task<ContactRecord> CreateForReport(Guid floodReportId, ContactRecordDto dto, CancellationToken ct)
    {
        var floodReport = await context.FloodReports
            .Include(o => o.ExtraContactRecords)
            .Where(o => o.Id == floodReportId)
            .FirstOrDefaultAsync(ct)
            .ConfigureAwait(false);

        if (floodReport == null)
        {
            throw new InvalidOperationException($"No flood report found");
        }

        // Only allow 1 instance of each type of contact to be created
        if (floodReport.ExtraContactRecords.Any(o => o.ContactType == dto.ContactType))
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
                PhoneNumber = dto.PhoneNumber
            };
            context.ContactRecords.Add(contactRecord);
            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        if (contactRecord?.Id == null)
        {
            throw new InvalidOperationException($"Contact record was not correctly created.");
        }

        // Do we need to add this contact record to the flood report?
        var existingLink = floodReport.ExtraContactRecords.Contains(contactRecord);
        if (existingLink == true)
        {
            throw new InvalidOperationException($"FloodReport already has a contact of type {dto.ContactType}.");
        } else
        {
            // Link the contact to the flood report
            floodReport.ExtraContactRecords.Add(contactRecord);
            await context.SaveChangesAsync().ConfigureAwait(false);
        } 

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
        contactRecord = contactRecord with
        {
            UpdatedUtc = DateTimeOffset.UtcNow,

            ContactUserId = userId,
            ContactType = dto.ContactType,
            ContactName = dto.ContactName,
            EmailAddress = dto.EmailAddress,
            PhoneNumber = dto.PhoneNumber,
        };
        await context.SaveChangesAsync(ct).ConfigureAwait(false);

        // Publish a created message to the message system
        foreach (FloodReport floodReport in contactRecord.FloodReports)
        {
            var message = contactRecord.ToMessageUpdated(floodReport.Reference);
            await publishEndpoint
                .Publish(message, ct)
                .ConfigureAwait(false);
        }

        // Update the contact record and add the message to the database
        await context
            .SaveChangesAsync(ct)
            .ConfigureAwait(false);

        return contactRecord;
    }

    public async Task DeleteForUser(Guid userId, Guid id, ContactRecordType contactType, CancellationToken ct)
    {
        var contactRecord = await context.ContactRecords
            .Where(fc => fc.ContactUserId == userId &&
                        fc.Id == id &&
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

        // Publish a deleted message to the message system
        foreach (FloodReport floodReport in contactRecord.FloodReports)
        {
            var message = contactRecord.ToMessageDeleted(floodReport.Reference);
            await publishEndpoint
                .Publish(message, ct)
                .ConfigureAwait(false);
        } 

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
            .SelectMany(o => o.ExtraContactRecords)
            .Select(o => o.ContactType)
            .ToListAsync(ct)
            .ConfigureAwait(false);

        return [..Enum.GetValues<ContactRecordType>().Where(o => o != ContactRecordType.Unknown && !usedRecordTypes.Contains(o))];
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
            .SelectMany(o => o.ExtraContactRecords)
            .Select(o => o.ContactType)
            .Distinct()
            .CountAsync(ct)
            .ConfigureAwait(false);

        // return the difference
        return allRecordTypes - usedRecordTypes;
    }
}

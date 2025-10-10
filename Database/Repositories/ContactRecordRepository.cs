using FloodOnlineReportingTool.Database.DbContexts;
using FloodOnlineReportingTool.Database.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace FloodOnlineReportingTool.Database.Repositories;

public class ContactRecordRepository(PublicDbContext context, IPublishEndpoint publishEndpoint) : IContactRecordRepository
{
    public async Task<ContactRecord?> ReportedByUser(Guid userId, Guid id, CancellationToken ct)
    {
        return await context.FloodReports
            .AsNoTracking()
            .Include(o => o.ContactRecords)
            .Where(o => o.ReportedByUserId == userId)
            .SelectMany(o => o.ContactRecords)
            .FirstOrDefaultAsync(o => o.Id == id, ct)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyCollection<ContactRecord>> AllReportedByUser(Guid userId, CancellationToken ct)
    {
        return await context.FloodReports
            .AsNoTracking()
            .Include(o => o.ContactRecords)
            .Where(o => o.ReportedByUserId == userId)
            .SelectMany(o => o.ContactRecords)
            .OrderByDescending(o => o.CreatedUtc)
            .ToListAsync(ct)
            .ConfigureAwait(false);
    }

    public async Task<ContactRecord> CreateForUser(Guid userId, ContactRecordDto dto, CancellationToken ct)
    {
        var floodReport = await context.FloodReports
            .AsNoTracking()
            .Include(o => o.ContactRecords)
            .Where(o => o.ReportedByUserId == userId)
            .FirstOrDefaultAsync(ct)
            .ConfigureAwait(false);

        if (floodReport == null)
        {
            throw new InvalidOperationException($"No flood report found for user");
        }

        // Only allow 1 type of contact to be created
        if (floodReport.ContactRecords.Any(o => o.ContactType == dto.ContactType))
        {
            throw new InvalidOperationException($"A contact record of type {dto.ContactType} already exists for the user");
        }

        // Create the new contact record.
        var now = DateTimeOffset.UtcNow;
        var contactRecord = new ContactRecord
        {
            CreatedUtc = now,
            RedactionDate = now.AddMonths(6), // This is the default redaction period

            ContactType = dto.ContactType,
            ContactName = dto.ContactName,
            EmailAddress = dto.EmailAddress,
            PhoneNumber = dto.PhoneNumber,
        };

        floodReport.ContactRecords.Add(contactRecord);

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
        // using 2 queries only because I need the flood report reference below when creating the message (I am sure this can be optimised)
        var floodReport = await context.FloodReports
            .AsNoTracking()
            .Where(o => o.ReportedByUserId == userId)
            .Select(o => new
            {
                o.Id,
                o.Reference,
            })
            .FirstOrDefaultAsync(ct)
            .ConfigureAwait(false);

        if (floodReport == null)
        {
            throw new InvalidOperationException("No flood report found");
        }

        var contactRecord = await context.ContactRecords
            .AsNoTracking()
            .Where(o => o.FloodReportIds.Contains(floodReport.Id))
            .FirstOrDefaultAsync(o => o.Id == id, ct)
            .ConfigureAwait(false);

        if (contactRecord == null)
        {
            throw new InvalidOperationException("No contact record found");
        }

        // Update the fields we choose
        var updatedRecord = contactRecord with
        {
            UpdatedUtc = DateTimeOffset.UtcNow,

            ContactType = dto.ContactType,
            ContactName = dto.ContactName,
            EmailAddress = dto.EmailAddress,
            PhoneNumber = dto.PhoneNumber,
        };

        context.ContactRecords.Update(updatedRecord);

        // Publish a created message to the message system
        var message = updatedRecord.ToMessageUpdated(floodReport.Reference);
        await publishEndpoint
            .Publish(message, ct)
            .ConfigureAwait(false);

        // Update the contact record and add the message to the database
        await context
            .SaveChangesAsync(ct)
            .ConfigureAwait(false);

        return updatedRecord;
    }

    public async Task DeleteForUser(Guid userId, Guid id, CancellationToken ct)
    {
        // using 2 queries only because I need the flood report reference below when creating the message (I am sure this can be optimised)
        var floodReport = await context.FloodReports
            .AsNoTracking()
            .Where(o => o.ReportedByUserId == userId)
            .Select(o => new
            {
                o.Id,
                o.Reference,
            })
            .FirstOrDefaultAsync(ct)
            .ConfigureAwait(false);

        if (floodReport == null)
        {
            throw new InvalidOperationException("No flood report found");
        }

        var contactRecord = await context.ContactRecords
            .Where(o => o.FloodReportIds.Contains(floodReport.Id))
            .FirstOrDefaultAsync(o => o.Id == id, ct)
            .ConfigureAwait(false);

        if (contactRecord == null)
        {
            throw new InvalidOperationException("No contact record found");
        }

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

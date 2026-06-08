using FloodOnlineReportingTool.Contracts.Shared;
using FloodOnlineReportingTool.Database.DbContexts;
using FloodOnlineReportingTool.Database.Models.Contact;
using FloodOnlineReportingTool.Database.Models.Flood;
using FloodOnlineReportingTool.Database.Models.ResultModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FloodOnlineReportingTool.Database.Repositories;

public class ContactRecordRepository(
    ILogger<ContactRecordRepository> logger, 
    IDbContextFactory<PublicDbContext> contextFactory
) : IContactRecordRepository
{
    private const int RedactionMonths = 6;

    public async Task<ContactRecord?> Get(Guid contactRecordId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting contact record by ID: {ContactRecordId}", contactRecordId);

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.ContactRecords
            .AsNoTracking()
            .IgnoreAutoIncludes()
            .Where(cr => cr.Id == contactRecordId)
            .OrderBy(cr => cr.Id)
            .Include(cr => cr.FloodReportSources)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ContactRecord?> Get(string userId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting contact record by user ID");

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.ContactRecords
            .AsNoTracking()
            .IgnoreAutoIncludes()
            .Where(cr => cr.ContactUserId == userId)
            .OrderBy(cr => cr.Id)
            .Include(cr => cr.FloodReportSources)
            .FirstOrDefaultAsync(cancellationToken);
    }
    

    public async Task<IReadOnlyCollection<ContactRecord>> GetContactsByReport(Guid floodReportSourceId, CancellationToken ct)
    {
        logger.LogInformation("Getting contact records for flood report source ID: {FloodReportSourceId}", floodReportSourceId);

        await using var context = await contextFactory.CreateDbContextAsync(ct);
        var floodReportSource = await context.FloodReportSources
            .AsNoTracking()
            .IgnoreAutoIncludes()
            .Include(frs => frs.ContactRecords)
                .ThenInclude(cr => cr.SubscribeRecords)
            .FirstOrDefaultAsync(frs => frs.Id == floodReportSourceId, ct);

        var contactRecords = floodReportSource?.ContactRecords.ToList();

        return contactRecords ?? [];
    }

    public async Task<Result<ContactRecord>> Create(string? userId, CancellationToken cancellationToken)
    {
        return await Create(userId, floodReportSourceId: null, cancellationToken);
    }

    public async Task<Result<ContactRecord>> Create(string? userId, Guid? floodReportSourceId, CancellationToken cancellationToken)
    {
        if (userId is null)
        {
            logger.LogInformation("Creating contact record");
        }
        else
        {
            logger.LogInformation("Creating contact record for a user");
        }

        var now = DateTimeOffset.UtcNow;
        ContactRecord contactRecord = new()
        {
            Id = Guid.CreateVersion7(),
            CreatedUtc = now,
            RedactionDate = now.AddMonths(RedactionMonths),
            ContactUserId = userId,
        };

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        // Check if a contact record already exists for the user
        bool contactRecordExistsForUser = userId is not null && await context.ContactRecords
            .AsNoTracking()
            .IgnoreAutoIncludes()
            .AnyAsync(cr => cr.ContactUserId == userId, cancellationToken);
        if (contactRecordExistsForUser)
        {
            logger.LogInformation("Contact record already exists for user, cannot create another for user");
            return Result<ContactRecord>.Failure(["Cannot create contact record for user, a record already exists"]);
        }

        if (floodReportSourceId.HasValue)
        {
            if (floodReportSourceId.Value == Guid.Empty)
            {
                logger.LogInformation("Empty flood report source ID provided for contact record");
                return Result<ContactRecord>.Failure(["Cannot create contact record with empty flood report ID"]);
            }

            FloodReportSource? floodReportSource = await context.FloodReportSources
                .IgnoreAutoIncludes()
                .FirstOrDefaultAsync(frs => frs.Id == floodReportSourceId.Value, cancellationToken);

            if (floodReportSource is null)
            {
                logger.LogInformation("Flood report source {FloodReportSourceId} not found for contact record", floodReportSourceId);
                return Result<ContactRecord>.Failure(["Cannot create contact record, no flood report found"]);
            }

            logger.LogInformation("New contact record for flood report source ID: {FloodReportSourceId}", floodReportSourceId);
            contactRecord.FloodReportSources.Add(floodReportSource);
        }

        context.ContactRecords.Add(contactRecord);
        await context.SaveChangesAsync(cancellationToken);

        return Result<ContactRecord>.Success(contactRecord);
    }

    public async Task<Result<ContactRecord>> LinkContactByReport(Guid floodReportSourceId, Guid contactRecordId, CancellationToken ct)
    {
        logger.LogInformation("Linking contact record ID: {ContactRecordId} and flood report source ID: {FloodReportSourceId}", contactRecordId, floodReportSourceId);

        await using var context = await contextFactory.CreateDbContextAsync(ct);
        var contactRecord = await context.ContactRecords
            .FirstOrDefaultAsync(cr => cr.Id == contactRecordId, ct);
        if (contactRecord is null)
        {
            logger.LogInformation("Contact record not found for linking, ID: {ContactRecordId}", contactRecordId);
            return Result<ContactRecord>.Failure(["Could not link contact record to flood report, the contact record was not found"]);
        }

        var floodReportSource = await context.FloodReportSources.FindAsync([floodReportSourceId], ct);
        if (floodReportSource is null)
        {
            logger.LogInformation("Flood report source not found for linking, ID: {FloodReportSourceId}", floodReportSourceId);
            return Result<ContactRecord>.Failure(["Could not link contact record to flood report, the flood report was not found"]);
        }

        contactRecord.FloodReportSources.Add(floodReportSource);

        await context.SaveChangesAsync(ct);

        return Result<ContactRecord>.Success(contactRecord);
    }

    public async Task<Result<ContactRecord>> UpdateForUser(string userId, Guid contactRecordId, CancellationToken ct)
    {
        logger.LogInformation("Updating contact record ID: {ContactRecordId} for user", contactRecordId);

        await using var context = await contextFactory.CreateDbContextAsync(ct);
        var contactRecord = await context.ContactRecords
            .FirstOrDefaultAsync(cr => cr.Id == contactRecordId && cr.ContactUserId == userId, ct);

        if (contactRecord == null)
        {
            return Result<ContactRecord>.Failure([ $"No contact record found for record type" ]);
        }

        contactRecord = contactRecord with
        {
            UpdatedUtc = DateTimeOffset.UtcNow,
            ContactUserId = userId,
        };

        context.Update(contactRecord);
        await context.SaveChangesAsync(ct);

        return Result<ContactRecord>.Success(contactRecord);
    }

    public async Task<DeleteResult<ContactRecord>> DeleteById(Guid contactRecordId, ContactRecordType contactType, CancellationToken ct)
    {
        logger.LogInformation("Deleting contact record ID: {ContactRecordId} of type: {ContactType}", contactRecordId, contactType);

        await using var context = await contextFactory.CreateDbContextAsync(ct);
        var contactRecord = await context.ContactRecords
            .Include(cr => cr.SubscribeRecords)
            .FirstOrDefaultAsync(cr => cr.Id == contactRecordId, ct);
        
        if (contactRecord == null)
        {
            return DeleteResult<ContactRecord>.Failure([ $"No contact record found for record type {contactType}" ]);
        }
        var subscribeRecordToRemove = contactRecord.SubscribeRecords
                .FirstOrDefault(sr => sr.ContactType == contactType);
        if (subscribeRecordToRemove == null)
        {
            return DeleteResult<ContactRecord>.Failure([$"No contact record found for record type {contactType}"]);
        }

        // Remove the contact record from the flood report source
        if (subscribeRecordToRemove.IsRecordOwner || contactRecord.SubscribeRecords.Count == 1)
        {
            // If this is the record owner or there is only one subscribe record, we can remove the whole contact record
            context.ContactRecords.Remove(contactRecord);
        }
        else
        {
            // If there are multiple subscribe records we only remove the one matching the contact type
            if (subscribeRecordToRemove != null)
            {
               context.ContactSubscribeRecords.Remove(subscribeRecordToRemove);
            }
        }

        // Remove the contact record and add the message to the database
        await context.SaveChangesAsync(ct);

        return DeleteResult<ContactRecord>.Success();
    }

    public async Task<IList<ContactRecordType>> GetUnusedRecordTypes(Guid floodReportSourceId, CancellationToken ct)
    {
        logger.LogInformation("Getting unused contact record types for flood report source ID: {FloodReportSourceId}", floodReportSourceId);

        await using var context = await contextFactory.CreateDbContextAsync(ct);
        var usedRecordTypes = await context.FloodReportSources
            .AsNoTracking()
            .IgnoreAutoIncludes()
            .Where(frs => frs.Id == floodReportSourceId)
            .SelectMany(frs => frs.ContactRecords)
            .SelectMany(cr => cr.SubscribeRecords)
            .Select(sr => sr.ContactType)
            .ToListAsync(ct);

        return [.. Enum.GetValues<ContactRecordType>().Where(o => o != ContactRecordType.Unknown && !usedRecordTypes.Contains(o))];
    }

    public async Task<int> Count(CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.ContactRecords.CountAsync(cancellationToken);
    }

    public async Task<int> Count(string userId, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.ContactRecords.CountAsync(cr => cr.ContactUserId == userId, cancellationToken);
    }

    public async Task<int> CountUnusedRecordTypes(Guid floodReportSourceId, CancellationToken ct)
    {
        logger.LogInformation("Counting unused contact record types for flood report source ID: {FloodReportSourceId}", floodReportSourceId);

        // get how many contact record types are in the enum
        var allRecordTypes = Enum.GetValues<ContactRecordType>().Count(o => o != ContactRecordType.Unknown);

        // get how many unique contact record types are in the database
        await using var context = await contextFactory.CreateDbContextAsync(ct);
        var usedRecordTypes = await context.FloodReportSources
            .AsNoTracking()
            .IgnoreAutoIncludes()
            .Where(frs => frs.Id == floodReportSourceId)
            .SelectMany(frs => frs.ContactRecords)
            .SelectMany(cr => cr.SubscribeRecords)
            .Select(sr => sr.ContactType)
            .Distinct()
            .CountAsync(ct);

        // return the difference
        return allRecordTypes - usedRecordTypes;
    }

    public async Task<bool> Exists(Guid contactRecordId, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.ContactRecords
            .AsNoTracking()
            .IgnoreAutoIncludes()
            .AnyAsync(cr => cr.Id == contactRecordId, cancellationToken);
    }

    public async Task<bool> Exists(string userId, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.ContactRecords
            .AsNoTracking()
            .IgnoreAutoIncludes()
            .AnyAsync(cr => cr.ContactUserId == userId, cancellationToken);
    }

    public async Task<bool> Exists(Guid floodReportSourceId, ContactRecordType contactRecordType, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.ContactRecords
            .AsNoTracking()
            .IgnoreAutoIncludes()
            .Where(cr => cr.FloodReportSources.Any(frs => frs.Id == floodReportSourceId) && cr.SubscribeRecords.Any(sr => sr.ContactType == contactRecordType))
            .AnyAsync(cancellationToken);
    }
}

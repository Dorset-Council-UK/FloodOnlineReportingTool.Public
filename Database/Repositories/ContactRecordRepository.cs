using FloodOnlineReportingTool.Contracts.Shared;
using FloodOnlineReportingTool.Database.DbContexts;
using FloodOnlineReportingTool.Database.Models.Contact;
using FloodOnlineReportingTool.Database.Models.Contact.Subscribe;
using MassTransit.Initializers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace FloodOnlineReportingTool.Database.Repositories;

public class ContactRecordRepository(ILogger<ContactRecordRepository> logger, IDbContextFactory<PublicDbContext> contextFactory) : IContactRecordRepository
{
    private const int VerificationExpiryMinutesUserPresent = 30;
    private const int VerificationExpiryMinutesUserNotPresent = 4320; // 3 days
    private const int RedactionDelayMinutes = 31;
    private const int RedactionPeriod = 6;

    public async Task<ContactRecord?> GetContactById(Guid contactRecordId, CancellationToken ct)
    {
        logger.LogInformation("Getting contact record by ID: {ContactRecordId}", contactRecordId);

        await using var context = await contextFactory.CreateDbContextAsync(ct);
        return await context.ContactRecords
            .AsNoTracking()
            .IgnoreAutoIncludes()
            .Where(cr => cr.Id == contactRecordId)
            .OrderBy(cr => cr.Id)
            .Include(cr => cr.FloodReports)
            .FirstOrDefaultAsync(ct);
    }
    
    public async Task<SubscribeRecord?> GetReportOwnerContactByReport(Guid floodReportId, CancellationToken ct)
    {
        logger.LogInformation("Getting report owner contact records for flood report ID: {FloodReportId}", floodReportId);

        await using var context = await contextFactory.CreateDbContextAsync(ct);
        var floodReport = await context.FloodReports
        .AsNoTracking()
        .IgnoreAutoIncludes()
        .Include(fr => fr.ContactRecords.OrderBy(cr => cr.Id))
            .ThenInclude(cr => cr.SubscribeRecords)
        .FirstOrDefaultAsync(fr => fr.Id == floodReportId, ct);

        var ownerSubscribeRecord = floodReport?.ContactRecords
        .SelectMany(cr => cr.SubscribeRecords)
        .FirstOrDefault(sr => sr.IsRecordOwner == true);

        return ownerSubscribeRecord ?? null;
    }

    public async Task<IReadOnlyCollection<ContactRecord>> GetContactsByReport(Guid floodReportId, CancellationToken ct)
    {
        logger.LogInformation("Getting contact records for flood report ID: {FloodReportId}", floodReportId);

        await using var context = await contextFactory.CreateDbContextAsync(ct);
        var floodReport = await context.FloodReports
        .AsNoTracking()
        .IgnoreAutoIncludes()
        .Include(fr => fr.ContactRecords)
            .ThenInclude(sr => sr.SubscribeRecords)
        .FirstOrDefaultAsync(fr => fr.Id == floodReportId, ct);

        var contactRecords = floodReport?.ContactRecords
            .ToList();

        return contactRecords ?? [];
    }

    public async Task<ContactRecordCreateOrUpdateResult> CreateForReport(Guid floodReportId, ContactRecordDto dto, CancellationToken ct)
    {
        logger.LogInformation("Creating contact record for flood report ID: {FloodReportId}", floodReportId);

        await using var context = await contextFactory.CreateDbContextAsync(ct);
        var floodReport = await context.FloodReports
            .IgnoreAutoIncludes()
            .Include(o => o.ContactRecords)
            .FirstOrDefaultAsync(o => o.Id == floodReportId, ct);

        if (floodReport == null)
        {
            return ContactRecordCreateOrUpdateResult.Failure([ $"No flood report found for ID {floodReportId}" ]);
        }

        // Does the user already have a contact record?
        if (dto.UserId != null)
        {
            Guid userId = dto.UserId.Value;
            var existingContactRecordId = await context.ContactRecords
                .AsNoTracking()
                .Where(cr => cr.ContactUserId == userId)
                .Select(cr => (Guid?)cr.Id)
                .FirstOrDefaultAsync(ct);
            if (existingContactRecordId != null)
            {
                logger.LogInformation("User ID: {UserId} already has a contact record ID: {ContactRecordId}, trying to update the record instead", userId, existingContactRecordId.Value);
                return await UpdateForUser(userId, existingContactRecordId.Value, dto, ct);
            }
        }

        var now = DateTimeOffset.UtcNow;
        var contactRecordId = Guid.CreateVersion7();

        ContactRecord contactRecord = new()
        {
            Id = contactRecordId,
            CreatedUtc = now,
            RedactionDate = now.AddMonths(RedactionPeriod),
            ContactUserId = dto.UserId,
            FloodReports = [floodReport],
        };

        // Add the contact record, including linking it to the flood report
        context.ContactRecords.Add(contactRecord);
        await context.SaveChangesAsync(ct);

        return ContactRecordCreateOrUpdateResult.Success(contactRecord);
    }

    public async Task<ContactRecordCreateOrUpdateResult> LinkContactByReport(Guid floodReportId, Guid contactRecordId, CancellationToken ct)
    {
        logger.LogInformation("Linking contact record ID: {ContactRecordId} with record {FloodReportId}", contactRecordId, floodReportId);

        await using var context = await contextFactory.CreateDbContextAsync(ct);
        var contactRecord = await context.ContactRecords
            .FirstOrDefaultAsync(cr => cr.Id == contactRecordId, ct);

        var floodReport = await context.FloodReports.FindAsync(floodReportId);
        if (floodReport == null || contactRecord == null)
        {
            return ContactRecordCreateOrUpdateResult.Failure(["Record not found."]);
        }

        contactRecord.FloodReports.Add(floodReport);

        await context.SaveChangesAsync(ct);

        return ContactRecordCreateOrUpdateResult.Success(contactRecord);
    }

    public async Task<ContactRecordCreateOrUpdateResult> UpdateForUser(Guid userId, Guid contactRecordId, ContactRecordDto dto, CancellationToken ct)
    {
        logger.LogInformation("Updating contact record ID: {ContactRecordId} for user ID: {UserId}", contactRecordId, userId);

        await using var context = await contextFactory.CreateDbContextAsync(ct);
        var contactRecord = await context.ContactRecords
            .FirstOrDefaultAsync(cr => cr.Id == contactRecordId && cr.ContactUserId == userId, ct);

        if (contactRecord == null)
        {
            return ContactRecordCreateOrUpdateResult.Failure([ $"No contact record found for record type" ]);
        }

        contactRecord = contactRecord with
        {
            UpdatedUtc = DateTimeOffset.UtcNow,

            ContactUserId = userId,
        };

        context.Update(contactRecord);
        await context.SaveChangesAsync(ct);

        return ContactRecordCreateOrUpdateResult.Success(contactRecord);
    }

    public async Task<ContactRecordDeleteResult> DeleteById(Guid contactRecordId, ContactRecordType contactType, CancellationToken ct)
    {
        logger.LogInformation("Deleting contact record ID: {ContactRecordId} of type: {ContactType}", contactRecordId, contactType);

        await using var context = await contextFactory.CreateDbContextAsync(ct);
        var contactRecord = await context.ContactRecords
            .FirstOrDefaultAsync(cr => cr.Id == contactRecordId, ct);
        
        if (contactRecord == null)
        {
            return ContactRecordDeleteResult.Failure([ $"No contact record found for record type {contactType}" ]);
        }
        var subscribeRecordToRemove = contactRecord.SubscribeRecords
                .FirstOrDefault(sr => sr.ContactType == contactType);
        if (subscribeRecordToRemove == null)
        {
            return ContactRecordDeleteResult.Failure([$"No contact record found for record type {contactType}"]);
        }

        // Remove the contact record from the flood report
        if (subscribeRecordToRemove.IsRecordOwner == true || contactRecord.SubscribeRecords.Count == 1)
        {
            // If this is the record owner or there is only one subscribe record, we can remove the whole contact record
            context.ContactRecords.Remove(contactRecord);
        } else
        {
            // If there are multiple subscribe records we only remove the one matching the contact type
            if (subscribeRecordToRemove != null)
            {
               context.ContactSubscribeRecords.Remove(subscribeRecordToRemove);
            }
        }

        // Remove the contact record and add the message to the database
        await context.SaveChangesAsync(ct);

        return ContactRecordDeleteResult.Success();
    }

    public async Task<SubscribeCreateOrUpdateResult> CreateSubscriptionRecord(Guid contactRecordId, ContactRecordDto dto, string? userEmail, bool userPresent, CancellationToken ct)
    {
        logger.LogInformation("Creating contact subscription record for email: {EmailAddress}", dto.EmailAddress);
        await using var context = await contextFactory.CreateDbContextAsync(ct);

        SubscribeRecord contactModel = new()
        {
            ContactName = dto.ContactName!,
            EmailAddress = dto.EmailAddress!,
            ContactType = dto.ContactType,
            ContactRecordId = contactRecordId,
        };

        // Assumption, logged in users do not need to authenticate unless it is not their email address.
        bool AutoVerifyEmail = false;
        if (!string.IsNullOrEmpty(userEmail))
        {
            if (string.Equals(userEmail, dto.EmailAddress, StringComparison.OrdinalIgnoreCase))
            {
                AutoVerifyEmail = true;
            }
        }

        // Only allow 1 instance of each type of contact to be created
        if (context.ContactSubscribeRecords.Where(csr => csr.ContactRecordId == contactRecordId).Any(o => o.ContactType == dto.ContactType))
        {
            return SubscribeCreateOrUpdateResult.Failure([$"A contact record of type {dto.ContactType} already exists for the user"]);
        }

        SubscribeRecord newSubscription = new SubscribeRecord()
        {
            ContactRecordId = contactRecordId,
            ContactType = dto.ContactType,
            ContactName = dto.ContactName,
            EmailAddress = dto.EmailAddress,
            IsEmailVerified = AutoVerifyEmail,   
            IsSubscribed = false,
            IsRecordOwner = dto.IsRecordOwner,
            CreatedUtc = DateTimeOffset.UtcNow,
            VerificationCode = RandomNumberGenerator.GetInt32(100000, 1000000),
            VerificationExpiryUtc = DateTimeOffset.UtcNow.AddMinutes((userPresent ? VerificationExpiryMinutesUserPresent : VerificationExpiryMinutesUserNotPresent)),
            RedactionDate = DateTimeOffset.UtcNow.AddMinutes(RedactionDelayMinutes)
        };

        context.ContactSubscribeRecords.Add(newSubscription);
        await context.SaveChangesAsync(ct);
        return SubscribeCreateOrUpdateResult.Success(newSubscription);
    }

    public async Task<SubscribeRecord?> GetSubscriptionRecordById(Guid subscriptionId, CancellationToken ct)
    {
        logger.LogInformation("Getting contact subscription record for id: {SubscriptionID}", subscriptionId);
        await using var context = await contextFactory.CreateDbContextAsync(ct);
        return await context.ContactSubscribeRecords
            .AsNoTracking()
            .IgnoreAutoIncludes()
            .Where(sr => sr.Id == subscriptionId)
            .Include(sr => sr.ContactRecord)
            .OrderByDescending(sr => sr.CreatedUtc)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<bool> VerifySubscriptionRecord(Guid subscriptionId, int verificationCode, CancellationToken ct)
    {
        logger.LogInformation("Verify subscription record for id: {SubscriptionID} by code", subscriptionId);
        await using var context = await contextFactory.CreateDbContextAsync(ct);
        var subscriptionRecord = await context.ContactSubscribeRecords
                .IgnoreAutoIncludes()
                .Where(sr => sr.Id == subscriptionId)
                .OrderByDescending(sr => sr.CreatedUtc)
                .FirstOrDefaultAsync(ct);

        if (subscriptionRecord == null)
        {
            return false;
        }

        if( subscriptionRecord.VerificationCode == verificationCode && subscriptionRecord.VerificationExpiryUtc >  DateTimeOffset.UtcNow)
        {
            // Mark as verified
            subscriptionRecord.IsEmailVerified = true;
            subscriptionRecord.VerificationCode = null;
            subscriptionRecord.VerificationExpiryUtc = null;

            await context.SaveChangesAsync(ct);
            return true;
        } else
        {
           return false;
        }

    }

    public async Task<SubscribeCreateOrUpdateResult> UpdateVerificationCode(SubscribeRecord subscriptionRecord, bool userPresent, CancellationToken ct)
    {
        logger.LogInformation("Updating contact subscription verification code for record ID: {SubscriptionId}", subscriptionRecord.Id);
        await using var context = await contextFactory.CreateDbContextAsync(ct);

        subscriptionRecord.VerificationCode = RandomNumberGenerator.GetInt32(100000, 1000000);
        subscriptionRecord.VerificationExpiryUtc = DateTimeOffset.UtcNow.AddMinutes((userPresent ? VerificationExpiryMinutesUserPresent : VerificationExpiryMinutesUserNotPresent));

        context.ContactSubscribeRecords.Update(subscriptionRecord);
        await context.SaveChangesAsync(ct);
        return SubscribeCreateOrUpdateResult.Success(subscriptionRecord);
    }

    public async Task<SubscribeCreateOrUpdateResult> UpdateSubscriptionRecord(SubscribeRecord subscriptionRecord, CancellationToken ct)
    {
        logger.LogInformation("Updating contact subscription record ID: {SubscriptionId}", subscriptionRecord.Id);
        await using var context = await contextFactory.CreateDbContextAsync(ct);

        var subscribeRecord = await context.ContactSubscribeRecords
            .FirstOrDefaultAsync(cr => cr.Id == subscriptionRecord.Id, ct);
        if (subscribeRecord == null)
        {
            return SubscribeCreateOrUpdateResult.Failure([$"No contact record found for ID {subscriptionRecord.ContactRecordId}"]);
        }

        if (subscriptionRecord.ContactType != subscribeRecord.ContactType && context.ContactSubscribeRecords.Any(o => o.ContactType == subscriptionRecord.ContactType && o.ContactRecord!.Id == subscriptionRecord.ContactRecordId))
        {
            return SubscribeCreateOrUpdateResult.Failure([$"A contact record of type {subscriptionRecord.ContactType} already exists for the user"]);
        }

        // Determine if the email has changed; if it has, we need to reset the verified status
        bool emailNotChanged = subscribeRecord.EmailAddress.Equals(subscriptionRecord.EmailAddress, StringComparison.OrdinalIgnoreCase);
        var isEmailVerified = emailNotChanged && (subscribeRecord.IsEmailVerified || subscriptionRecord.IsEmailVerified);

        subscribeRecord.IsRecordOwner = subscriptionRecord.IsRecordOwner;
        subscribeRecord.ContactType = subscriptionRecord.ContactType;
        subscribeRecord.IsSubscribed = subscriptionRecord.IsSubscribed;
        subscribeRecord.IsEmailVerified = isEmailVerified;
        subscribeRecord.EmailAddress = subscriptionRecord.EmailAddress;
        subscribeRecord.ContactName = subscriptionRecord.ContactName;
        subscribeRecord.PhoneNumber = subscriptionRecord.PhoneNumber;

        await context.SaveChangesAsync(ct);
        return SubscribeCreateOrUpdateResult.Success(subscriptionRecord);
    }

    public async Task<SubscribeDeleteResult> DeleteSubscriptionById(Guid subscriptionId, CancellationToken ct)
    {
        logger.LogInformation("Deleting subscription record ID: {SubscriptionId}", subscriptionId);

        await using var context = await contextFactory.CreateDbContextAsync(ct);
        var subscriptionRecord = await context.ContactSubscribeRecords
            .Where(sr => sr.Id == subscriptionId)
            .FirstOrDefaultAsync(ct);

        if (subscriptionRecord == null)
        {
            return SubscribeDeleteResult.Failure([$"No subscription record found"]);
        }

        // Remove the subscription record from the flood report
        context.ContactSubscribeRecords.Remove(subscriptionRecord);

        // Remove the subscription record and add the message to the database
        await context.SaveChangesAsync(ct);

        return SubscribeDeleteResult.Success();
    }


    public async Task<IList<ContactRecordType>> GetUnusedRecordTypes(Guid floodReportId, CancellationToken ct)
    {
        logger.LogInformation("Getting unused contact record types for flood report ID: {FloodReportId}", floodReportId);

        await using var context = await contextFactory.CreateDbContextAsync(ct);
        var usedRecordTypes = await context.FloodReports
            .AsNoTracking()
            .IgnoreAutoIncludes()
            .Where(f => f.Id == floodReportId)
            .SelectMany(f => f.ContactRecords)
            .SelectMany(cr => cr.SubscribeRecords)
            .Select(sr => sr.ContactType)
            .ToListAsync(ct);

        return [.. Enum.GetValues<ContactRecordType>().Where(o => o != ContactRecordType.Unknown && !usedRecordTypes.Contains(o))];
    }

    public async Task<int> CountUnusedRecordTypes(Guid floodReportId, CancellationToken ct)
    {
        logger.LogInformation("Counting unused contact record types for flood report ID: {FloodReportId}", floodReportId);

        // get how many contact record types are in the enum
        var allRecordTypes = Enum.GetValues<ContactRecordType>().Count(o => o != ContactRecordType.Unknown);

        // get how many unique contact record types are in the database
        await using var context = await contextFactory.CreateDbContextAsync(ct);
        var usedRecordTypes = await context.FloodReports
            .AsNoTracking()
            .IgnoreAutoIncludes()
            .Where(f => f.Id == floodReportId)
            .SelectMany(f => f.ContactRecords)
            .SelectMany(cr => cr.SubscribeRecords)
            .Select(sr => sr.ContactType)
            .Distinct()
            .CountAsync(ct);

        // return the difference
        return allRecordTypes - usedRecordTypes;
    }

    public async Task<bool> ContactRecordExists(Guid contactRecordId, CancellationToken ct = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);
        return await context.ContactRecords
            .AsNoTracking()
            .AnyAsync(o => o.Id == contactRecordId, ct);
    }

    public async Task<Guid?> ContactRecordExistsForUser(Guid userId, CancellationToken ct = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);
        return await context.ContactRecords
            .AsNoTracking()
            .Where(o => o.ContactUserId == userId)
            .Select(o => o.Id)
            .FirstOrDefaultAsync(ct);
    }
}

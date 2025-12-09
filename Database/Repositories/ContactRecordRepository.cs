using FloodOnlineReportingTool.Contracts.Shared;
using FloodOnlineReportingTool.Database.DbContexts;
using FloodOnlineReportingTool.Database.Models.Contact;
using FloodOnlineReportingTool.Database.Models.Contact.Subscribe;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Cryptography;

namespace FloodOnlineReportingTool.Database.Repositories;

public class ContactRecordRepository(ILogger<ContactRecordRepository> logger, IDbContextFactory<PublicDbContext> contextFactory) : IContactRecordRepository
{
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

    public async Task<IReadOnlyCollection<ContactRecord>> GetContactsByReport(Guid floodReportId, CancellationToken ct)
    {
        logger.LogInformation("Getting contact records for flood report ID: {FloodReportId}", floodReportId);

        await using var context = await contextFactory.CreateDbContextAsync(ct);
        var floodReport = await context.FloodReports
        .AsNoTracking()
        .Include(fr => fr.ContactRecords.OrderBy(cr => cr.Id))
            .ThenInclude(cr => cr.SubscribeRecord)
        .FirstOrDefaultAsync(fr => fr.Id == floodReportId, ct);

        return floodReport?.ContactRecords.ToList() ?? [];
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

        // Only allow 1 instance of each type of contact to be created
        if (floodReport.ContactRecords.Any(o => o.ContactType == dto.ContactType))
        {
            return ContactRecordCreateOrUpdateResult.Failure([ $"A contact record of type {dto.ContactType} already exists for the user" ]);
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
        
        // Load the SubscribeRecord
        var subscribeRecord = await context.ContactSubscribeRecords
            .FirstOrDefaultAsync(sr => sr.Id == dto.SubscribeRecord.Id, ct);
        
        if (subscribeRecord == null)
        {
            return ContactRecordCreateOrUpdateResult.Failure([ $"No subscribe record found for ID {dto.SubscribeRecord.Id}" ]);
        }

        ContactRecord contactRecord = new()
        {
            Id = contactRecordId,
            CreatedUtc = now,
            RedactionDate = now.AddMonths(6),
            ContactUserId = dto.UserId,
            ContactType = dto.ContactType,
            PhoneNumber = dto.PhoneNumber,
            FloodReports = [floodReport],
        };

        // Link the relationship from both sides
        subscribeRecord.ContactRecordId = contactRecordId;
        subscribeRecord.ContactRecord = contactRecord;

        if (contactRecord.ContactType == ContactRecordType.HomeOwner)
        {
            floodReport.ReportOwnerId = contactRecordId;
        }

        // Add the contact record, including linking it to the flood report
        context.ContactRecords.Add(contactRecord);
        context.Update(subscribeRecord);
        await context.SaveChangesAsync(ct);

        return ContactRecordCreateOrUpdateResult.Success(contactRecord);
    }

    public async Task<ContactRecordCreateOrUpdateResult> UpdateForUser(Guid userId, Guid contactRecordId, ContactRecordDto dto, CancellationToken ct)
    {
        logger.LogInformation("Updating contact record ID: {ContactRecordId} for user ID: {UserId}", contactRecordId, userId);

        await using var context = await contextFactory.CreateDbContextAsync(ct);
        var contactRecord = await context.ContactRecords
            .FirstOrDefaultAsync(cr => cr.Id == contactRecordId && cr.ContactUserId == userId && cr.ContactType == dto.ContactType, ct);

        if (contactRecord == null)
        {
            return ContactRecordCreateOrUpdateResult.Failure([ $"No contact record found for record type {dto.ContactType}" ]);
        }

        if (contactRecord.ContactType != dto.ContactType)
        {
            return ContactRecordCreateOrUpdateResult.Failure([ $"The contact record type cannot be changed from {contactRecord.ContactType} to {dto.ContactType}" ]);
        }

        // Determine if the email has changed; if it has, we need to reset the verified status
        bool emailNotChanged = contactRecord.SubscribeRecord.EmailAddress.Equals(dto.EmailAddress, StringComparison.OrdinalIgnoreCase);
        var isEmailVerified = emailNotChanged && (contactRecord.SubscribeRecord.IsEmailVerified || dto.IsEmailVerified);

        contactRecord = contactRecord with
        {
            UpdatedUtc = DateTimeOffset.UtcNow,

            ContactUserId = userId,
            //ContactType = dto.ContactType,
            //ContactName = dto.ContactName,
            //EmailAddress = dto.EmailAddress,
            //IsEmailVerified = isEmailVerified,
            PhoneNumber = dto.PhoneNumber,
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
            .Where(cr => cr.Id == contactRecordId && cr.ContactType == contactType)
            .FirstOrDefaultAsync(ct);

        if (contactRecord == null)
        {
            return ContactRecordDeleteResult.Failure([ $"No contact record found for record type {contactType}" ]);
        }

        // Remove the contact record from the flood report
        context.ContactRecords.Remove(contactRecord);

        // Update the flood report owner if the contact type was home owner
        if (contactType == ContactRecordType.HomeOwner)
        {
            var floodReport = await context.FloodReports
                .IgnoreAutoIncludes()
                .FirstOrDefaultAsync(f => f.ReportOwnerId == contactRecordId, ct);

            if (floodReport != null)
            {
                floodReport.ReportOwnerId = null;
                context.Update(floodReport);
            }
        }

        // Remove the contact record and add the message to the database
        await context.SaveChangesAsync(ct);

        return ContactRecordDeleteResult.Success();
    }

    public async Task<SubscribeCreateOrUpdateResult> CreateSubscriptionRecord(SubscribeRecord contactSubscription, bool isUserAuthenticated, CancellationToken ct)
    {
        logger.LogInformation("Creating contact subscription record for email: {EmailAddress}", contactSubscription.EmailAddress);

        await using var context = await contextFactory.CreateDbContextAsync(ct);
        SubscribeRecord newSubscription = new SubscribeRecord()
        {
            ContactName = contactSubscription.ContactName,
            EmailAddress = contactSubscription.EmailAddress,
            IsEmailVerified = isUserAuthenticated, //Assumption, logged in users do not need to authenticate unless it is not their email address. This logic is in calling function. 
            IsSubscribed = false,
            CreatedUtc = DateTimeOffset.UtcNow,
            VerificationCode = RandomNumberGenerator.GetInt32(100000, 1000000),
            VerificationExpiryUtc = DateTimeOffset.UtcNow.AddMinutes(30),
            RedactionDate = DateTimeOffset.UtcNow.AddMinutes(31)
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

    public async Task<SubscribeCreateOrUpdateResult> UpdateVerificationCode(SubscribeRecord subscriptionRecord, CancellationToken ct)
    {
        logger.LogInformation("Updating contact subscription verification code for record ID: {SubscriptionId}", subscriptionRecord.Id);
        await using var context = await contextFactory.CreateDbContextAsync(ct);

        subscriptionRecord.VerificationCode = RandomNumberGenerator.GetInt32(100000, 1000000);
        subscriptionRecord.VerificationExpiryUtc = DateTimeOffset.UtcNow.AddMinutes(30);

        context.ContactSubscribeRecords.Update(subscriptionRecord);
        await context.SaveChangesAsync(ct);
        return SubscribeCreateOrUpdateResult.Success(subscriptionRecord);
    }

    public async Task<SubscribeCreateOrUpdateResult> UpdateSubscriptionRecord(SubscribeRecord subscriptionRecord, CancellationToken ct)
    {
        logger.LogInformation("Updating contact subscription record ID: {SubscriptionId}", subscriptionRecord.Id);
        await using var context = await contextFactory.CreateDbContextAsync(ct);
        
        context.ContactSubscribeRecords.Update(subscriptionRecord);
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
            .Select(cr => cr.ContactType)
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
            .Select(cr => cr.ContactType)
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

    public async Task<bool> ContactRecordExistsForUser(Guid userId, CancellationToken ct = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);
        return await context.ContactRecords
            .AsNoTracking()
            .AnyAsync(o => o.ContactUserId == userId, ct);
    }
}

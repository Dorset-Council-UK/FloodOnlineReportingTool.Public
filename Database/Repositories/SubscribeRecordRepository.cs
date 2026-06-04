using FloodOnlineReportingTool.Database.DbContexts;
using FloodOnlineReportingTool.Database.Models.Contact.Subscribe;
using FloodOnlineReportingTool.Database.Models.ResultModels;
using FloodOnlineReportingTool.Database.Services;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace FloodOnlineReportingTool.Database.Repositories;

public class SubscribeRecordRepository(
    IDbContextFactory<PublicDbContext> contextFactory,
    IUserContext userContext
) : ISubscribeRecordRepository
{
    private const int VerificationExpiryMinutesUserPresent = 30;
    private const int VerificationExpiryMinutesUserNotPresent = 4320; // 3 days
    private const int RedactionDelayMinutes = 31;

    public async Task<int> Count(CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.ContactSubscribeRecords.CountAsync(cancellationToken);
    }

    public async Task<int> Count(Guid contactRecordId, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.ContactSubscribeRecords.CountAsync(sr => sr.ContactRecordId == contactRecordId, cancellationToken);
    }

    public async Task<int> Count(string userId, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.ContactSubscribeRecords
            .Include(sr => sr.ContactRecord)
            .CountAsync(sr => sr.ContactRecord != null && sr.ContactRecord.ContactUserId == userId, cancellationToken);
    }

    public async Task<bool> Exists(Guid subscribeRecordId, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.ContactSubscribeRecords.AnyAsync(sr => sr.Id == subscribeRecordId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<SubscribeRecord>> GetAll(Guid contactRecordId, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.ContactSubscribeRecords
            .AsNoTracking()
            .Where(sr => sr.ContactRecordId == contactRecordId)
            .OrderByDescending(sr => sr.CreatedUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<SubscribeRecord>> GetAll(CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.ContactSubscribeRecords
            .AsNoTracking()
            .OrderByDescending(sr => sr.CreatedUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<SubscribeRecord?> Get(Guid subscriptionId, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.ContactSubscribeRecords
            .AsNoTracking()
            .Where(sr => sr.Id == subscriptionId)
            .Include(sr => sr.ContactRecord)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<SubscribeRecord?> Get(Guid contactRecordId, Guid subscribeRecordId, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.ContactSubscribeRecords
            .AsNoTracking()
            .Where(sr => sr.ContactRecordId == contactRecordId && sr.Id == subscribeRecordId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<SubscribeRecord?> GetReportOwnerContactByReport(Guid floodReportId, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var floodReport = await context.FloodReports
            .AsNoTracking()
            .IgnoreAutoIncludes()
            .Include(fr => fr.ContactRecords.OrderBy(cr => cr.Id))
                .ThenInclude(cr => cr.SubscribeRecords)
            .FirstOrDefaultAsync(fr => fr.Id == floodReportId, cancellationToken);

        if (floodReport is null)
        {
            return null;
        }

        var ownerSubscribeRecord = floodReport.ContactRecords
            .SelectMany(cr => cr.SubscribeRecords)
            .RoleBasedFilterPersonalData(userContext.CanViewPersonalData)
            .FirstOrDefault(sr => sr.IsRecordOwner);

        return ownerSubscribeRecord ?? null;
    }

    public async Task<Result<SubscribeRecord>> Create(Guid contactRecordId, SubscribeRecordDto subscribeRecordDto, string? userEmail, bool userPresent, CancellationToken cancellationToken)
    {
        // Assumption, logged in users do not need to authenticate unless it is not their email address.
        bool isEmailVerified = string.Equals(userEmail, subscribeRecordDto.EmailAddress, StringComparison.OrdinalIgnoreCase);

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        // Only allow 1 instance of each type of contact to be created
        var contactTypeAlreadyExists = await context.ContactSubscribeRecords
            .Where(sr => sr.ContactRecordId == contactRecordId && sr.ContactType == subscribeRecordDto.ContactType)
            .AnyAsync(cancellationToken);
        if (contactTypeAlreadyExists)
        {
            return Result<SubscribeRecord>.Failure([$"A contact record of type {subscribeRecordDto.ContactType} already exists for the user"]);
        }

        SubscribeRecord newSubscription = new()
        {
            ContactRecordId = contactRecordId,
            ContactType = subscribeRecordDto.ContactType,
            ContactName = subscribeRecordDto.ContactName,
            EmailAddress = subscribeRecordDto.EmailAddress,
            PhoneNumber = subscribeRecordDto.PhoneNumber,
            IsEmailVerified = isEmailVerified,
            IsSubscribed = false,
            IsRecordOwner = subscribeRecordDto.IsRecordOwner,
            CreatedUtc = DateTimeOffset.UtcNow,
            VerificationCode = RandomNumberGenerator.GetInt32(100000, 1000000),
            VerificationExpiryUtc = DateTimeOffset.UtcNow.AddMinutes(userPresent ? VerificationExpiryMinutesUserPresent : VerificationExpiryMinutesUserNotPresent),
            RedactionDate = DateTimeOffset.UtcNow.AddMinutes(RedactionDelayMinutes),
        };

        context.ContactSubscribeRecords.Add(newSubscription);
        await context.SaveChangesAsync(cancellationToken);

        return Result<SubscribeRecord>.Success(newSubscription);
    }

    public async Task<Result<SubscribeRecord>> Update(Guid subscribeRecordId, SubscribeRecordDto subscribeRecordDto, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var subscribeRecord = await context.ContactSubscribeRecords
            .AsNoTracking()
            .FirstOrDefaultAsync(sr => sr.Id == subscribeRecordId, cancellationToken);
        if (subscribeRecord is null)
        {
            return Result<SubscribeRecord>.Failure([$"Cannot update subscription record. No subscription record found for ID {subscribeRecordId}"]);
        }

        var contactTypeAlreadyExists = await context.ContactSubscribeRecords
            .Where(sr => sr.ContactRecordId == subscribeRecord.ContactRecordId && sr.ContactType == subscribeRecordDto.ContactType && sr.Id != subscribeRecordId)
            .AnyAsync(cancellationToken);
        if (contactTypeAlreadyExists)
        {
            return Result<SubscribeRecord>.Failure([$"A contact record of type {subscribeRecordDto.ContactType} already exists"]);
        }

        // Determine if the email has changed; if it has, we need to reset the verified status
        bool emailChanged = !string.Equals(subscribeRecord.EmailAddress, subscribeRecordDto.EmailAddress, StringComparison.OrdinalIgnoreCase);
        var isEmailVerified = !emailChanged && subscribeRecord.IsEmailVerified;

        subscribeRecord.IsRecordOwner = subscribeRecordDto.IsRecordOwner;
        subscribeRecord.ContactType = subscribeRecordDto.ContactType;
        subscribeRecord.IsEmailVerified = isEmailVerified;
        subscribeRecord.EmailAddress = subscribeRecordDto.EmailAddress;
        subscribeRecord.ContactName = subscribeRecordDto.ContactName;
        subscribeRecord.PhoneNumber = subscribeRecordDto.PhoneNumber;

        context.ContactSubscribeRecords.Update(subscribeRecord);
        await context.SaveChangesAsync(cancellationToken);

        return Result<SubscribeRecord>.Success(subscribeRecord);
    }

    public async Task<Result<SubscribeRecord>> UpdateVerificationCode(SubscribeRecord subscriptionRecord, bool userPresent, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        subscriptionRecord.VerificationCode = RandomNumberGenerator.GetInt32(100000, 1000000);
        subscriptionRecord.VerificationExpiryUtc = DateTimeOffset.UtcNow.AddMinutes(userPresent ? VerificationExpiryMinutesUserPresent : VerificationExpiryMinutesUserNotPresent);

        context.ContactSubscribeRecords.Update(subscriptionRecord);
        await context.SaveChangesAsync(cancellationToken);

        return Result<SubscribeRecord>.Success(subscriptionRecord);
    }

    public async Task<DeleteResult<SubscribeRecord>> Delete(Guid subscribeRecordId, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        
        var subscribeRecord = await context.ContactSubscribeRecords.FindAsync([subscribeRecordId], cancellationToken);
        if (subscribeRecord is null)
        {
            return DeleteResult<SubscribeRecord>.Failure([$"No subscription record found"]);
        }

        context.ContactSubscribeRecords.Remove(subscribeRecord);
        await context.SaveChangesAsync(cancellationToken);

        return DeleteResult<SubscribeRecord>.Success();
    }


    public async Task<bool> Verify(Guid subscribeRecordId, int verificationCode, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var subscribeRecord = await context.ContactSubscribeRecords.FindAsync([subscribeRecordId], cancellationToken);
        if (subscribeRecord is null)
        {
            return false;
        }

        if (subscribeRecord.VerificationCode == verificationCode && subscribeRecord.VerificationExpiryUtc > DateTimeOffset.UtcNow)
        {
            // Mark as verified
            subscribeRecord.IsEmailVerified = true;
            subscribeRecord.VerificationCode = null;
            subscribeRecord.VerificationExpiryUtc = null;

            context.ContactSubscribeRecords.Update(subscribeRecord);
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }

        return false;
    }
}

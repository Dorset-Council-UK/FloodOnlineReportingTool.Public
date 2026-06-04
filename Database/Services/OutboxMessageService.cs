using FloodOnlineReportingTool.Database.DbContexts;
using FloodOnlineReportingTool.Database.Models.Messaging;
using Microsoft.EntityFrameworkCore;

namespace FloodOnlineReportingTool.Database.Services;

public class OutboxMessageService(IDbContextFactory<PublicDbContext> contextFactory) : IOutboxMessageService
{
    public async Task<IReadOnlyCollection<OutboxMessage>> Get(CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.OutboxMessages
            .AsNoTracking()
            .OrderBy(m => m.Priority)
            .ThenBy(m => m.Created)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<OutboxMessage>> Get(MessageStatus messageStatus, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.OutboxMessages
            .AsNoTracking()
            .Where(m => m.Status == messageStatus)
            .OrderBy(m => m.Priority)
            .ThenBy(m => m.Created)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<OutboxMessage>> Get(MessageStatus[] messageStatuses, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.OutboxMessages
            .AsNoTracking()
            .Where(m => messageStatuses.Contains(m.Status))
            .OrderBy(m => m.Priority)
            .ThenBy(m => m.Created)
            .ToListAsync(cancellationToken);
    }

    public async Task<OutboxMessage?> Get(Guid id, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.OutboxMessages.FindAsync([id], cancellationToken);
    }

    public async Task<int> Count(CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.OutboxMessages.CountAsync(cancellationToken);
    }

    public async Task<int> Count(MessageStatus messageStatus, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.OutboxMessages.CountAsync(m => m.Status == messageStatus, cancellationToken);
    }

    public async Task<int> Count(MessageStatus[] messageStatuses, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.OutboxMessages.CountAsync(m => messageStatuses.Contains(m.Status), cancellationToken);
    }
}

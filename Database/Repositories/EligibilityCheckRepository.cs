using FloodOnlineReportingTool.Database.DbContexts;
using FloodOnlineReportingTool.Database.Models.Eligibility;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FloodOnlineReportingTool.Database.Repositories;

public class EligibilityCheckRepository(
    ILogger<EligibilityCheckRepository> logger,
    IDbContextFactory<PublicDbContext> contextFactory
) : IEligibilityCheckRepository
{
    public async Task<EligibilityCheck?> ReportedByUser(string userId, CancellationToken ct)
    {
        // TODO: Might need to check a status on the flood report
        await using var context = await contextFactory.CreateDbContextAsync(ct);
        return await context.ContactRecords
            .AsNoTracking()
            .Where(cr => cr.ContactUserId == userId)
            .SelectMany(cr => cr.FloodReports)
            .Select(fr => fr.EligibilityCheck)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<EligibilityCheck?> ReportedByUser(string userId, Guid id, CancellationToken ct)
    {
        // TODO: Might need to check a status on the flood report
        await using var context = await contextFactory.CreateDbContextAsync(ct);
        return await context.ContactRecords
            .AsNoTracking()
            .Where(cr => cr.ContactUserId == userId)
            .SelectMany(cr => cr.FloodReports)
            .Select(fr => fr.EligibilityCheck)
            .FirstOrDefaultAsync(o => o != null && o.Id == id, ct);
    }

    public async Task<EligibilityCheck?> GetById(Guid id, CancellationToken ct)
    {
        logger.LogInformation("Getting eligibility check by id {Id}", id);

        // FloodProblems, and FloodImpacts will be auto included
        await using var context = await contextFactory.CreateDbContextAsync(ct);
        return await context.EligibilityChecks.FindAsync([id], ct);
    }

    public async Task<EligibilityCheck?> GetByReference(string reference, CancellationToken ct)
    {
        logger.LogInformation("Getting eligibility check by flood report reference {Reference}", reference.Replace(Environment.NewLine, "", StringComparison.OrdinalIgnoreCase));

        await using var context = await contextFactory.CreateDbContextAsync(ct);
        return await context.EligibilityChecks
            .AsNoTracking()
            .Include(o => o.FloodReport)
            .Where(o => o.FloodReport != null && o.FloodReport.Reference == reference)
            .FirstOrDefaultAsync(ct);
    }
}

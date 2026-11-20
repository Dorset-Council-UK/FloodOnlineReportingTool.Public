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
        await using var context = await contextFactory.CreateDbContextAsync(ct);
        return await context.ContactRecords
            .AsNoTracking()
            .Where(cr => cr.ContactUserId == userId)
            .SelectMany(cr => cr.FloodReportSources)
            .Select(frs => frs.EligibilityCheck)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<EligibilityCheck?> ReportedByUser(string userId, Guid eligibilityCheckId, CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);
        return await context.ContactRecords
            .AsNoTracking()
            .Where(cr => cr.ContactUserId == userId)
            .SelectMany(cr => cr.FloodReportSources)
            .Select(frs => frs.EligibilityCheck)
            .FirstOrDefaultAsync(ec => ec != null && ec.Id == eligibilityCheckId, ct);
    }

    public async Task<EligibilityCheck?> ReportedByUser(string userId, string reference, CancellationToken ct)
    {
        var floodReportSourceReference = reference.ReplaceLineEndings("");
        logger.LogInformation("Getting eligibility check by user ID and flood report source reference {Reference}", floodReportSourceReference);

        await using var context = await contextFactory.CreateDbContextAsync(ct);
        return await context.ContactRecords
            .AsNoTracking()
            .Where(cr => cr.ContactUserId == userId)
            .SelectMany(cr => cr.FloodReportSources)
            .Where(frs => frs.EligibilityCheckId != null && frs.Reference == floodReportSourceReference)
            .Select(frs => frs.EligibilityCheck)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<EligibilityCheck?> GetById(Guid eligibilityCheckId, CancellationToken ct)
    {
        logger.LogInformation("Getting eligibility check by id {EligibilityCheckId}", eligibilityCheckId);

        // FloodProblems, and FloodImpacts will be auto included
        await using var context = await contextFactory.CreateDbContextAsync(ct);
        return await context.EligibilityChecks.FindAsync([eligibilityCheckId], ct);
    }

    public async Task<EligibilityCheck?> GetByReference(string reference, CancellationToken ct)
    {
        logger.LogInformation("Getting eligibility check by flood report source reference {Reference}", reference.Replace(Environment.NewLine, "", StringComparison.OrdinalIgnoreCase));

        await using var context = await contextFactory.CreateDbContextAsync(ct);
        return await context.EligibilityChecks
            .AsNoTracking()
            .Include(ec => ec.FloodReportSource)
            .Include(navigationPropertyPath: ec => ec.FloodReportSource.ContactRecords)
            .Where(ec => ec.FloodReportSource != null && ec.FloodReportSource.Reference == reference)
            .FirstOrDefaultAsync(ct);
    }
}

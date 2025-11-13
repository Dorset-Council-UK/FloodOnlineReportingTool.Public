using Bogus;
using FloodOnlineReportingTool.Contracts;
using FloodOnlineReportingTool.Contracts.Shared;
using FloodOnlineReportingTool.Database.DbContexts;
using FloodOnlineReportingTool.Database.Models.Eligibility;
using FloodOnlineReportingTool.Database.Models.Flood;
using FloodOnlineReportingTool.Database.Models.Flood.FloodProblemIds;
using FloodOnlineReportingTool.Database.Repositories;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace FloodOnlineReportingTool.Public.Services;

public sealed class TestService(
    IPublishEndpoint publishEndpoint,
    IDbContextFactory<PublicDbContext> contextFactory,
    ICommonRepository commonRepository,
    IFloodReportRepository floodReportRepository
) {
    internal async Task TestMessage(CancellationToken ct)
    {
#if DEBUG
        // Make a test message for the EligibilityCheckCreated event
        Randomizer.Seed = new Random(232589734);

        var eligibilityCheckOrganisationFaker = new Faker<EligibilityCheckOrganisation>("en_GB")
            .CustomInstantiator(f => new(
                f.Random.Uuid(),
                f.Company.CompanyName(),
                f.Random.Uuid(),
                f.Commerce.ProductName()
            ));

        var eligibilityCheckSourceFaker = new Faker<EligibilityCheckFloodSource>("en_GB")
            .CustomInstantiator(f => new(
                f.Random.Uuid(),
                f.Company.CompanyName()
            ));

        var eligibilityCheckCreatedFaker = new Faker<EligibilityCheckCreated>("en_GB")
            .CustomInstantiator(f => new(
                f.Random.Uuid(),
                f.Random.Hexadecimal(8, "").ToUpperInvariant(),
                f.Date.RecentOffset(),
                f.Random.Long(1, 9999999999),
                f.Random.Long(1, 9999999999),
                f.Random.Double(0, 700000),
                f.Random.Double(0, 1300000),
                f.Date.PastOffset(),
                f.Random.Int(1, 72),
                f.Random.Bool(),
                f.Random.Bool(),
                f.Random.Int(0, 5),
                f.Company.CompanyName(),
                eligibilityCheckOrganisationFaker.GenerateBetween<EligibilityCheckOrganisation>(1, 3),
                eligibilityCheckSourceFaker.GenerateBetween<EligibilityCheckFloodSource>(1, 3)
            ));

        var message = eligibilityCheckCreatedFaker.Generate();

        await using var context = await contextFactory.CreateDbContextAsync(ct);
        await publishEndpoint.Publish(message, ct);
        await context.SaveChangesAsync(ct);
#else
        await Task.CompletedTask;
#endif
    }

    internal async Task<string?> TestFloodReport(CancellationToken ct)
    {
#if DEBUG
        EligibilityCheckDto dto = new()
        {
            Uprn = 10023242411,
            Usrn = 20023242411,
            Easting = 368991.12,
            Northing = 90881.94,
            IsAddress = true,
            LocationDesc = "TEST location description",
            TemporaryUprn = null,
            TemporaryLocationDesc = null,
            ImpactStart = DateTimeOffset.UtcNow.AddDays(-1),
            DurationKnownId = FloodDurationIds.Duration24,
            ImpactDuration = 24,
            OnGoing = false,
            Uninhabitable = false,
            VulnerablePeopleId = Database.Models.Status.RecordStatusIds.Yes,
            VulnerableCount = 1,
            Residentials = [
                FloodImpactIds.InsideLivingArea,
                FloodImpactIds.Basement,
            ],
            Commercials = [
                FloodImpactIds.InsideBuilding,
                FloodImpactIds.CarPark,
            ],
            Sources = [
                PrimaryCauseIds.River,
                PrimaryCauseIds.WaterRisingOutOfTheGround,
            ],
            SecondarySources = [
                SecondaryCauseIds.RunoffFromRoad,
                SecondaryCauseIds.RunoffFromTrackOrPath,
            ],
        };

        var floodReport = await floodReportRepository.CreateWithEligiblityCheck(dto, ct);

        if (floodReport is null)
        {
            return null;
        }

        return floodReport.Reference;
#else
        await Task.CompletedTask;
        return null;
#endif
    }
}

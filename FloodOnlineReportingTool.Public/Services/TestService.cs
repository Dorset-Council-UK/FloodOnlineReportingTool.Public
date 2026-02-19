using Bogus;
using FloodOnlineReportingTool.Contracts;
using FloodOnlineReportingTool.Contracts.Shared;
using FloodOnlineReportingTool.Database.DbContexts;
using FloodOnlineReportingTool.Database.Models.Eligibility;
using FloodOnlineReportingTool.Database.Models.Flood;
using FloodOnlineReportingTool.Database.Models.Flood.FloodProblemIds;
using FloodOnlineReportingTool.Database.Models.Investigate;
using FloodOnlineReportingTool.Database.Repositories;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace FloodOnlineReportingTool.Public.Services;

public sealed class TestService(
    IPublishEndpoint publishEndpoint,
    IDbContextFactory<PublicDbContext> contextFactory,
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

    internal static async Task<Investigation?> TestInvestigation(CancellationToken ct)
    {
#if DEBUG
        var now = DateTimeOffset.UtcNow;

        return new()
        {
            Id = Guid.CreateVersion7(),
            CreatedUtc = now,

            // Water speed
            BeginId = Guid.Empty, // FloodProblem
            WaterSpeedId = Guid.Empty, // FloodProblem
            AppearanceId = Guid.Empty, // FloodProblem
            MoreAppearanceDetails = "TEST The water looked like it was made of strawberry milkshake",

            // Internal how / Water entry
            Entries = [], // InvestigationEntry
            WaterEnteredOther = "TEST The shower is wet but I think thats normal",

            // Internal when
            WhenWaterEnteredKnownId = null, // RecordStatus
            FloodInternalUtc = null,

            // Water destination
            Destinations = [], // InvestigationDestination

            // Damaged vehicles
            WereVehiclesDamagedId = Guid.Empty, // RecordStatus
            NumberOfVehiclesDamaged = 6,

            // Peak depth
            IsPeakDepthKnownId = Guid.Empty, // RecordStatus
            PeakInsideCentimetres = null,
            PeakOutsideCentimetres = null,

            // Community impacts
            CommunityImpacts = [], // InvestigationCommunityImpact

            // Blockages
            HasKnownProblems = false, // TODO fix
            KnownProblemDetails = "TEST The drain was blocked with legos",

            // Actions taken
            ActionsTaken = [], // InvestigationActionsTaken
            OtherAction = "TEST I built a lego dam to stop the water, there was even fire engines!!",

            // Warnings - Help received
            HelpReceived = [], // InvestigationHelpReceived

            // Warnings ??
            FloodlineId = Guid.Empty, // RecordStatus
            WarningReceivedId = Guid.Empty, // RecordStatus

            // Warnings - Sources
            WarningSources = [], // InvestigationWarningSource
            WarningSourceOther = "TEST Many people were screaming, shouting, and letting it all out in the street",

            // Warnings - Floodline
            WarningTimelyId = null, // RecordStatus
            WarningAppropriateId = null, // RecordStatus

            // History
            HistoryOfFloodingId = Guid.Empty, // RecordStatus
            HistoryOfFloodingDetails = "TEST My brother broke the sink when he was 3 and flooded the bathroom",
        };
#else
        await Task.CompletedTask;
        return null;
#endif
    }

    internal static async Task<InvestigationDto?> TestInvestigationDto(CancellationToken ct)
    {
        var investigation = await TestInvestigation(ct);
        return investigation?.ToDto();
    }

    internal async Task TestFloodReportActionNeededStatus(Guid floodReportId, CancellationToken ct)
    {
#if DEBUG
        await using var context = await contextFactory.CreateDbContextAsync(ct);
        var floodReport = await context.FloodReports.FindAsync([floodReportId], ct);
        if (floodReport is null)
        {
            return;
        }

        if (floodReport.StatusId != RecordStatusIds.ActionNeeded)
        {
            floodReport.StatusId = RecordStatusIds.ActionNeeded;
            await context.SaveChangesAsync(ct);
        }
#else
        await Task.CompletedTask;
#endif
    }
}

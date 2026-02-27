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

#pragma warning disable MA0051 // Method is too long
#pragma warning disable CA1822 // Mark members as static

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

    internal async Task<Investigation?> TestInvestigation(CancellationToken ct)
    {
#if DEBUG
        var now = DateTimeOffset.UtcNow;
        var investigationId = Guid.CreateVersion7();

        return new()
        {
            Id = investigationId,
            CreatedUtc = now,

            // Water speed (FloodProblem's)
            BeginId = FloodOnsetIds.Gradually,
            WaterSpeedId = FloodSpeedIds.Slow,
            AppearanceId = FloodAppearanceIds.Muddy,
            MoreAppearanceDetails = "TEST The water looked like it was made of strawberry milkshake",

            // Internal how / Water entry (FloodProblem's)
            Entries = [
                new(investigationId, FloodEntryIds.Windows),
                new(investigationId, FloodEntryIds.Walls),
                new(investigationId, FloodEntryIds.ExternalOnly),
                new(investigationId, FloodEntryIds.Other),
            ],
            WaterEnteredOther = "TEST The shower is wet but I think thats normal",

            // Internal when (RecordStatus)
            WhenWaterEnteredKnownId = Database.Models.Status.RecordStatusIds.Yes,
            FloodInternalUtc = now.AddDays(-2),

            // Water destination (FloodProblem's)
            Destinations = [
                new(investigationId, FloodDestinationIds.StreamOrWatercourse),
                new(investigationId, FloodDestinationIds.DitchesAndDrainageChannels),
            ],

            // Damaged vehicles (RecordStatus)
            WereVehiclesDamagedId = Database.Models.Status.RecordStatusIds.Yes,
            NumberOfVehiclesDamaged = 6,

            // Peak depth (RecordStatus)
            IsPeakDepthKnownId = Database.Models.Status.RecordStatusIds.Yes,
            PeakInsideCentimetres = 35,
            PeakOutsideCentimetres = 50,

            // Service impacts (FloodImpact's)
            ServiceImpacts = [
                new(investigationId, FloodImpactIds.MainsSewer),
                new(investigationId, FloodImpactIds.Gas),
                new(investigationId, FloodImpactIds.Phoneline),
            ],

            // Community impacts (FloodImpact's)
            CommunityImpacts = [
                new(investigationId, FloodImpactIds.SomeRoadAccessBlocked),
                new(investigationId, FloodImpactIds.PublicTransportDisrupted),
            ],

            // Blockages
            HasKnownProblems = true,
            KnownProblemDetails = "TEST The drain was blocked with legos",

            // Actions taken (FloodMitigation's)
            ActionsTaken = [
                new(investigationId, FloodMitigationIds.SandlessSandbag),
                new(investigationId, FloodMitigationIds.FloodDoor),
                new(investigationId, FloodMitigationIds.AirBrickCover),
                new(investigationId, FloodMitigationIds.MoveValuables),
                new(investigationId, FloodMitigationIds.OtherAction),
            ],
            OtherAction = "TEST I built a lego dam to stop the water, there was even fire engines!!",

            // Warnings - Help received (FloodMitigation's)
            HelpReceived = [
                new(investigationId, FloodMitigationIds.WardenVolunteerHelp),
                new(investigationId, FloodMitigationIds.EnvironmentAgency),
                new(investigationId, FloodMitigationIds.LocalAuthority),
                new(investigationId, FloodMitigationIds.FloodlineHelp),
            ],

            // Warnings - Before the flooding (RecordStatus)
            FloodlineId = Database.Models.Status.RecordStatusIds.Yes,
            WarningReceivedId = Database.Models.Status.RecordStatusIds.Yes,

            // Warnings - Sources (FloodMitigation's)
            WarningSources = [
                new(investigationId, FloodMitigationIds.FloodlineWarning),
                new(investigationId, FloodMitigationIds.Radio),
                new(investigationId, FloodMitigationIds.WardenVolunteerHelp),
                new(investigationId, FloodMitigationIds.OtherWarning),
            ],
            WarningSourceOther = "TEST Many people were screaming, shouting, and letting it all out in the street",

            // Warnings - Floodline (RecordStatus)
            WarningTimelyId = Database.Models.Status.RecordStatusIds.No,
            WarningAppropriateId = Database.Models.Status.RecordStatusIds.No,

            // History (RecordStatus)
            HistoryOfFloodingId = Database.Models.Status.RecordStatusIds.Yes,
            HistoryOfFloodingDetails = "TEST My brother broke the sink when he was 3 and flooded the bathroom",
            PropertyInsuredId = Database.Models.Status.RecordStatusIds.Yes,
        };
#else
        await Task.CompletedTask;
        return null;
#endif
    }

    internal async Task<InvestigationDto?> TestInvestigationDto(CancellationToken ct)
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

using FloodOnlineReportingTool.Contracts;
using FloodOnlineReportingTool.Contracts.Shared;
using FloodOnlineReportingTool.Contracts.Shared.Models;
using FloodOnlineReportingTool.Database.DbContexts;
using FloodOnlineReportingTool.Database.Models.Contact;
using FloodOnlineReportingTool.Database.Models.Eligibility;
using FloodOnlineReportingTool.Database.Models.Flood;
using FloodOnlineReportingTool.Database.Models.Flood.FloodProblemIds;
using FloodOnlineReportingTool.Database.Models.Investigate;
using FloodOnlineReportingTool.Database.Models.Messaging;
using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.Public.Models.FloodReport.Create;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FloodOnlineReportingTool.Public.Services;

internal sealed class TestService(
    IDbContextFactory<PublicDbContext> contextFactory,
    IContactRecordRepository contactRecordRepository,
    IFloodReportRepository floodReportRepository,
    IHostEnvironment environment
) : ITestService
{
    private readonly JsonSerializerOptions _jsonOptions = JsonSerializerOptions.Web;

    public EligibilityCheckDto TestData_EligibilityCheckDto => new()
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

    private static readonly EligibilityCheckRecord TestData_EligibilityCheckRecord = new(
        Id: Guid.CreateVersion7(),
        Uprn: 10023242411,
        Usrn: 20023242411,
        Easting: 368991.12,
        Northing: 90881.94,
        ImpactStartUTC: DateTimeOffset.UtcNow.AddDays(-1),
        ImpactDurationHours: 24,
        IsOnGoing: false,
        IsUninhabitable: false,
        VulnerableCount: 1,
        LocationDescription: "TEST location description",
        Organisations: [
            new EligibilityCheckOrganisation(OrganisationIds.Dorset, "TEST Dorset Council", FloodAuthorityIds.LeadLocalFloodAuthority, "TEST LLFA"),
            new EligibilityCheckOrganisation(OrganisationIds.Wessex, "TEST Environment Agency (Wessex)", FloodAuthorityIds.EnvironmentAgency, "TEST EA"),
        ],
        FloodSources: [
            new EligibilityCheckFloodSource(PrimaryCauseIds.River, "TEST River"),
            new EligibilityCheckFloodSource(PrimaryCauseIds.WaterRisingOutOfTheGround, "TEST Water rising out of the ground"),
            new EligibilityCheckFloodSource(SecondaryCauseIds.RunoffFromRoad, "TEST Runoff from road"),
            new EligibilityCheckFloodSource(SecondaryCauseIds.RunoffFromTrackOrPath, "TEST Runoff from track/path"),
        ]
    );

    public ExtraData TestData_EligibilityCheck_ExtraData => new()
    {
        Postcode = "DT1 1XJ",
        PrimaryClassification = "TEST Commercial",
        SecondaryClassification = "TEST Office",
        PropertyType = FloodImpactIds.Commercial,
        TemporaryPostcode = null,
    };

    private static Investigation TestData_Investigation
    {
        get
        {
            var investigationId = Guid.CreateVersion7();
            var now = DateTimeOffset.UtcNow;

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
        }
    }

    public InvestigationDto TestData_InvestigationDto => TestData_Investigation.ToDto();

    private static readonly ContactRecordDto TestData_ContactRecordDtoDto = new()
    {
        ContactType = ContactRecordType.Tenant,
        ContactName = "TEST name",
        EmailAddress = "test@test.com",
    };

    public async Task<FloodReport?> TestFloodReport_Create(CancellationToken cancellationToken)
    {
        if (!environment.IsDevelopment())
        {
            return null;
        }

        var viewUriBase = new Uri("https://localhost:7039/report-flooding/test");
        var createResult = await floodReportRepository.Create(TestData_EligibilityCheckDto, viewUriBase, cancellationToken);
        return createResult.IsSuccess ? createResult.Value : null;
    }

    public async Task<FloodReport?> TestFloodReport_GetLast(string userId, CancellationToken cancellationToken)
    {
        if (!environment.IsDevelopment())
        {
            return null;
        }

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.ContactRecords
            .AsNoTracking()
            .AsSplitQuery()
            .Where(cr => cr.ContactUserId == userId)
            .SelectMany(cr => cr.FloodReports)
            .Include(fr => fr.ContactRecords)
            .Include(fr => fr.EligibilityCheck)
            .Include(fr => fr.Investigation)
            .Include(fr => fr.Status)
            .OrderByDescending(fr => fr.CreatedUtc)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ContactRecord?> TestContactRecord_Create(Guid floodReportId, string userId, CancellationToken cancellationToken)
    {
        if (!environment.IsDevelopment())
        {
            return null;
        }

        var dto = TestData_ContactRecordDtoDto with
        {
            UserId = userId,
        };

        var createResult = await contactRecordRepository.CreateForReport(floodReportId, dto, cancellationToken);
        return createResult.IsSuccess ? createResult.Value : null;
    }

    public async Task TestFloodReportActionNeededStatus(Guid floodReportId, CancellationToken ct)
    {
        if (!environment.IsDevelopment())
        {
            return;
        }

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
    }

    public async Task<OutboxMessage?> TestOutboxMessage_FloodReportSourceCreated(MessageStatus messageStatus, CancellationToken cancellationToken)
    {
        if (!environment.IsDevelopment())
        {
            return null;
        }

        var now = DateTimeOffset.UtcNow;

        FloodReportSourceCreated message = new(
            Id: Guid.CreateVersion7(),
            Buffer: 25,
            Reference: "TEST1234",
            ViewUri: new Uri("https://localhost:7039/report-flooding/test"),
            CreatedUtc: now,
            EligibilityCheckRecord: TestData_EligibilityCheckRecord,
            HasInvestigation: false,
            HasContacts: false,
            ContactRecordTypes: []
        );

        OutboxMessage outboxMessage = new()
        {
            Created = now,
            Status = messageStatus,
            Priority = MessagePriority.Low,
            MessageType = nameof(FloodReportSourceCreated),
            Message = JsonSerializer.Serialize(message, _jsonOptions),
        };

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        context.OutboxMessages.Add(outboxMessage);
        await context.SaveChangesAsync(cancellationToken);

        return outboxMessage;
    }

    public async Task<OutboxMessage?> TestOutboxMessage_FloodReportSourceUpdated(MessageStatus messageStatus, CancellationToken cancellationToken)
    {
        if (!environment.IsDevelopment())
        {
            return null;
        }

        var now = DateTimeOffset.UtcNow;

        FloodReportSourceUpdated message = new(
            Id: Guid.CreateVersion7(),
            Reference: "TEST1234",
            ViewUri: new Uri("https://localhost:7039/report-flooding/test"),
            UpdatedUtc: now,
            RecordStatusUpdate: Guid.Empty, // TODO: When FloodReportRepository.Update is used, create a real record status update and use its ID here
            EligibilityCheckRecord: TestData_EligibilityCheckRecord,
            ActionStatusUpdates: []
        );

        OutboxMessage outboxMessage = new()
        {
            Created = now,
            Status = messageStatus,
            Priority = MessagePriority.Low,
            MessageType = nameof(FloodReportSourceUpdated),
            Message = JsonSerializer.Serialize(message, _jsonOptions),
        };

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        context.OutboxMessages.Add(outboxMessage);
        await context.SaveChangesAsync(cancellationToken);

        return outboxMessage;
    }
}

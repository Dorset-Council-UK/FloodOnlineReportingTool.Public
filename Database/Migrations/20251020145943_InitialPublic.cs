using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FloodOnlineReportingTool.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialPublic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "fortpublic");

            migrationBuilder.CreateTable(
                name: "FloodAuthorities",
                schema: "fortpublic",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorityName = table.Column<string>(type: "text", nullable: false),
                    AuthorityDescription = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloodAuthorities", x => x.Id);
                },
                comment: "Authorities and agencies responsible for managing, mitigating, and responding to flood risks and incidents");

            migrationBuilder.CreateTable(
                name: "FloodImpacts",
                schema: "fortpublic",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: true),
                    TypeName = table.Column<string>(type: "text", nullable: true),
                    TypeDescription = table.Column<string>(type: "text", nullable: true),
                    CategoryPriority = table.Column<string>(type: "text", nullable: true),
                    OptionOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloodImpacts", x => x.Id);
                },
                comment: "Represents the broader impacts of a flood, such as health risks, economic losses, etc");

            migrationBuilder.CreateTable(
                name: "FloodMitigations",
                schema: "fortpublic",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: true),
                    TypeName = table.Column<string>(type: "text", nullable: true),
                    TypeDescription = table.Column<string>(type: "text", nullable: true),
                    OptionOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloodMitigations", x => x.Id);
                },
                comment: "Actions and measures taken to reduce or prevent the impact of flooding");

            migrationBuilder.CreateTable(
                name: "FloodProblems",
                schema: "fortpublic",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: true),
                    TypeName = table.Column<string>(type: "text", nullable: true),
                    TypeDescription = table.Column<string>(type: "text", nullable: true),
                    OptionOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloodProblems", x => x.Id);
                },
                comment: "Flood problems related to the occurrence, cause, or characteristics of a flood.");

            migrationBuilder.CreateTable(
                name: "InboxState",
                schema: "fortpublic",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConsumerId = table.Column<Guid>(type: "uuid", nullable: false),
                    LockId = table.Column<Guid>(type: "uuid", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true),
                    Received = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReceiveCount = table.Column<int>(type: "integer", nullable: false),
                    ExpirationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Consumed = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Delivered = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastSequenceNumber = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InboxState", x => x.Id);
                    table.UniqueConstraint("AK_InboxState_MessageId_ConsumerId", x => new { x.MessageId, x.ConsumerId });
                });

            migrationBuilder.CreateTable(
                name: "OutboxState",
                schema: "fortpublic",
                columns: table => new
                {
                    OutboxId = table.Column<Guid>(type: "uuid", nullable: false),
                    LockId = table.Column<Guid>(type: "uuid", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Delivered = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastSequenceNumber = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxState", x => x.OutboxId);
                });

            migrationBuilder.CreateTable(
                name: "RecordStatuses",
                schema: "fortpublic",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Text = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecordStatuses", x => x.Id);
                },
                comment: "Status used in various places including flood reports.");

            migrationBuilder.CreateTable(
                name: "Organisations",
                schema: "fortpublic",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    DataProtectionStatement = table.Column<string>(type: "text", nullable: true),
                    EmergencyPlanning = table.Column<string>(type: "text", nullable: true),
                    FloodAuthorityId = table.Column<Guid>(type: "uuid", nullable: false),
                    GettingInTouch = table.Column<string>(type: "text", nullable: true),
                    Logo = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    SubmissionReply = table.Column<string>(type: "text", nullable: true),
                    Website = table.Column<string>(type: "text", nullable: false),
                    LastUpdatedUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organisations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Organisations_FloodAuthorities_FloodAuthorityId",
                        column: x => x.FloodAuthorityId,
                        principalSchema: "fortpublic",
                        principalTable: "FloodAuthorities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Represents specific geographic administrative areas, responsible for flood management");

            migrationBuilder.CreateTable(
                name: "FloodAuthorityFloodProblems",
                schema: "fortpublic",
                columns: table => new
                {
                    FloodAuthorityId = table.Column<Guid>(type: "uuid", nullable: false),
                    FloodProblemId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloodAuthorityFloodProblems", x => new { x.FloodAuthorityId, x.FloodProblemId });
                    table.ForeignKey(
                        name: "FK_FloodAuthorityFloodProblems_FloodProblems_FloodProblemId",
                        column: x => x.FloodProblemId,
                        principalSchema: "fortpublic",
                        principalTable: "FloodProblems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Relationships between flood authorities and flood problems");

            migrationBuilder.CreateTable(
                name: "OutboxMessage",
                schema: "fortpublic",
                columns: table => new
                {
                    SequenceNumber = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EnqueueTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SentTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Headers = table.Column<string>(type: "text", nullable: true),
                    Properties = table.Column<string>(type: "text", nullable: true),
                    InboxMessageId = table.Column<Guid>(type: "uuid", nullable: true),
                    InboxConsumerId = table.Column<Guid>(type: "uuid", nullable: true),
                    OutboxId = table.Column<Guid>(type: "uuid", nullable: true),
                    MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContentType = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    MessageType = table.Column<string>(type: "text", nullable: false),
                    Body = table.Column<string>(type: "text", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uuid", nullable: true),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: true),
                    InitiatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceAddress = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    DestinationAddress = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ResponseAddress = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    FaultAddress = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ExpirationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessage", x => x.SequenceNumber);
                    table.ForeignKey(
                        name: "FK_OutboxMessage_InboxState_InboxMessageId_InboxConsumerId",
                        columns: x => new { x.InboxMessageId, x.InboxConsumerId },
                        principalSchema: "fortpublic",
                        principalTable: "InboxState",
                        principalColumns: new[] { "MessageId", "ConsumerId" });
                    table.ForeignKey(
                        name: "FK_OutboxMessage_OutboxState_OutboxId",
                        column: x => x.OutboxId,
                        principalSchema: "fortpublic",
                        principalTable: "OutboxState",
                        principalColumn: "OutboxId");
                });

            migrationBuilder.CreateTable(
                name: "EligibilityChecks",
                schema: "fortpublic",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Uprn = table.Column<long>(type: "bigint", nullable: true),
                    Usrn = table.Column<long>(type: "bigint", nullable: true),
                    Easting = table.Column<double>(type: "double precision", nullable: false),
                    Northing = table.Column<double>(type: "double precision", nullable: false),
                    IsAddress = table.Column<bool>(type: "boolean", nullable: false),
                    LocationDesc = table.Column<string>(type: "text", nullable: true),
                    TemporaryUprn = table.Column<long>(type: "bigint", nullable: true),
                    TemporaryLocationDesc = table.Column<string>(type: "text", nullable: true),
                    ImpactStart = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ImpactDuration = table.Column<int>(type: "integer", nullable: false),
                    OnGoing = table.Column<bool>(type: "boolean", nullable: false),
                    Uninhabitable = table.Column<bool>(type: "boolean", nullable: false),
                    VulnerablePeopleId = table.Column<Guid>(type: "uuid", nullable: false, defaultValue: new Guid("018feada-34c0-7e10-a183-7a5161c397dc")),
                    VulnerableCount = table.Column<int>(type: "integer", nullable: true),
                    TermsAgreed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EligibilityChecks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EligibilityChecks_RecordStatuses_VulnerablePeopleId",
                        column: x => x.VulnerablePeopleId,
                        principalSchema: "fortpublic",
                        principalTable: "RecordStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Eligibility assessments to determine if a person qualifies for assistance, related to flood damage");

            migrationBuilder.CreateTable(
                name: "Investigations",
                schema: "fortpublic",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    BeginId = table.Column<Guid>(type: "uuid", nullable: false),
                    WaterSpeedId = table.Column<Guid>(type: "uuid", nullable: false),
                    AppearanceId = table.Column<Guid>(type: "uuid", nullable: false),
                    MoreAppearanceDetails = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    WereVehiclesDamagedId = table.Column<Guid>(type: "uuid", nullable: false),
                    NumberOfVehiclesDamaged = table.Column<byte>(type: "smallint", nullable: true),
                    WaterEnteredOther = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    WhenWaterEnteredKnownId = table.Column<Guid>(type: "uuid", nullable: true),
                    FloodInternalUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsPeakDepthKnownId = table.Column<Guid>(type: "uuid", nullable: false),
                    PeakInsideCentimetres = table.Column<int>(type: "integer", nullable: true),
                    PeakOutsideCentimetres = table.Column<int>(type: "integer", nullable: true),
                    HasKnownProblems = table.Column<bool>(type: "boolean", nullable: false),
                    KnownProblemDetails = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    OtherAction = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FloodlineId = table.Column<Guid>(type: "uuid", nullable: false),
                    WarningReceivedId = table.Column<Guid>(type: "uuid", nullable: false),
                    WarningSourceOther = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    WarningTimelyId = table.Column<Guid>(type: "uuid", nullable: true),
                    WarningAppropriateId = table.Column<Guid>(type: "uuid", nullable: true),
                    HistoryOfFloodingId = table.Column<Guid>(type: "uuid", nullable: false),
                    HistoryOfFloodingDetails = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Investigations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Investigations_FloodProblems_AppearanceId",
                        column: x => x.AppearanceId,
                        principalSchema: "fortpublic",
                        principalTable: "FloodProblems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Investigations_FloodProblems_BeginId",
                        column: x => x.BeginId,
                        principalSchema: "fortpublic",
                        principalTable: "FloodProblems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Investigations_FloodProblems_WaterSpeedId",
                        column: x => x.WaterSpeedId,
                        principalSchema: "fortpublic",
                        principalTable: "FloodProblems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Investigations_RecordStatuses_FloodlineId",
                        column: x => x.FloodlineId,
                        principalSchema: "fortpublic",
                        principalTable: "RecordStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Investigations_RecordStatuses_HistoryOfFloodingId",
                        column: x => x.HistoryOfFloodingId,
                        principalSchema: "fortpublic",
                        principalTable: "RecordStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Investigations_RecordStatuses_IsPeakDepthKnownId",
                        column: x => x.IsPeakDepthKnownId,
                        principalSchema: "fortpublic",
                        principalTable: "RecordStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Investigations_RecordStatuses_WarningAppropriateId",
                        column: x => x.WarningAppropriateId,
                        principalSchema: "fortpublic",
                        principalTable: "RecordStatuses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Investigations_RecordStatuses_WarningReceivedId",
                        column: x => x.WarningReceivedId,
                        principalSchema: "fortpublic",
                        principalTable: "RecordStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Investigations_RecordStatuses_WarningTimelyId",
                        column: x => x.WarningTimelyId,
                        principalSchema: "fortpublic",
                        principalTable: "RecordStatuses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Investigations_RecordStatuses_WereVehiclesDamagedId",
                        column: x => x.WereVehiclesDamagedId,
                        principalSchema: "fortpublic",
                        principalTable: "RecordStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Investigations_RecordStatuses_WhenWaterEnteredKnownId",
                        column: x => x.WhenWaterEnteredKnownId,
                        principalSchema: "fortpublic",
                        principalTable: "RecordStatuses",
                        principalColumn: "Id");
                },
                comment: "Investigations, for example grants");

            migrationBuilder.CreateTable(
                name: "FloodResponsibilities",
                schema: "fortpublic",
                columns: table => new
                {
                    OrganisationId = table.Column<Guid>(type: "uuid", nullable: false),
                    AdminUnitId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    LookupDate = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloodResponsibilities", x => new { x.OrganisationId, x.AdminUnitId });
                    table.ForeignKey(
                        name: "FK_FloodResponsibilities_Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalSchema: "fortpublic",
                        principalTable: "Organisations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Areas responsible for handling flood reports/eligibility checks.");

            migrationBuilder.CreateTable(
                name: "EligibilityCheckCommercials",
                schema: "fortpublic",
                columns: table => new
                {
                    EligibilityCheckId = table.Column<Guid>(type: "uuid", nullable: false),
                    FloodImpactId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EligibilityCheckCommercials", x => new { x.EligibilityCheckId, x.FloodImpactId });
                    table.ForeignKey(
                        name: "FK_EligibilityCheckCommercials_EligibilityChecks_EligibilityCh~",
                        column: x => x.EligibilityCheckId,
                        principalSchema: "fortpublic",
                        principalTable: "EligibilityChecks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EligibilityCheckCommercials_FloodImpacts_FloodImpactId",
                        column: x => x.FloodImpactId,
                        principalSchema: "fortpublic",
                        principalTable: "FloodImpacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Relationships between eligibility checks and commercial flood impacts");

            migrationBuilder.CreateTable(
                name: "EligibilityCheckResidentials",
                schema: "fortpublic",
                columns: table => new
                {
                    EligibilityCheckId = table.Column<Guid>(type: "uuid", nullable: false),
                    FloodImpactId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EligibilityCheckResidentials", x => new { x.EligibilityCheckId, x.FloodImpactId });
                    table.ForeignKey(
                        name: "FK_EligibilityCheckResidentials_EligibilityChecks_EligibilityC~",
                        column: x => x.EligibilityCheckId,
                        principalSchema: "fortpublic",
                        principalTable: "EligibilityChecks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EligibilityCheckResidentials_FloodImpacts_FloodImpactId",
                        column: x => x.FloodImpactId,
                        principalSchema: "fortpublic",
                        principalTable: "FloodImpacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Relationships between eligibility checks and residential flood impacts");

            migrationBuilder.CreateTable(
                name: "EligibilityCheckRunoffSource",
                schema: "fortpublic",
                columns: table => new
                {
                    EligibilityCheckId = table.Column<Guid>(type: "uuid", nullable: false),
                    FloodProblemId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EligibilityCheckRunoffSource", x => new { x.EligibilityCheckId, x.FloodProblemId });
                    table.ForeignKey(
                        name: "FK_EligibilityCheckRunoffSource_EligibilityChecks_EligibilityC~",
                        column: x => x.EligibilityCheckId,
                        principalSchema: "fortpublic",
                        principalTable: "EligibilityChecks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EligibilityCheckRunoffSource_FloodProblems_FloodProblemId",
                        column: x => x.FloodProblemId,
                        principalSchema: "fortpublic",
                        principalTable: "FloodProblems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Relationships between eligibility checks and source runoff flood problems");

            migrationBuilder.CreateTable(
                name: "EligibilityCheckSources",
                schema: "fortpublic",
                columns: table => new
                {
                    EligibilityCheckId = table.Column<Guid>(type: "uuid", nullable: false),
                    FloodProblemId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EligibilityCheckSources", x => new { x.EligibilityCheckId, x.FloodProblemId });
                    table.ForeignKey(
                        name: "FK_EligibilityCheckSources_EligibilityChecks_EligibilityCheckId",
                        column: x => x.EligibilityCheckId,
                        principalSchema: "fortpublic",
                        principalTable: "EligibilityChecks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EligibilityCheckSources_FloodProblems_FloodProblemId",
                        column: x => x.FloodProblemId,
                        principalSchema: "fortpublic",
                        principalTable: "FloodProblems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Relationships between eligibility checks and source flood problems");

            migrationBuilder.CreateTable(
                name: "InvestigationActionsTaken",
                schema: "fortpublic",
                columns: table => new
                {
                    InvestigationId = table.Column<Guid>(type: "uuid", nullable: false),
                    FloodMitigationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvestigationActionsTaken", x => new { x.InvestigationId, x.FloodMitigationId });
                    table.ForeignKey(
                        name: "FK_InvestigationActionsTaken_FloodMitigations_FloodMitigationId",
                        column: x => x.FloodMitigationId,
                        principalSchema: "fortpublic",
                        principalTable: "FloodMitigations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InvestigationActionsTaken_Investigations_InvestigationId",
                        column: x => x.InvestigationId,
                        principalSchema: "fortpublic",
                        principalTable: "Investigations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Relationships between investigation and actions taken for the flood");

            migrationBuilder.CreateTable(
                name: "InvestigationCommunityImpact",
                schema: "fortpublic",
                columns: table => new
                {
                    InvestigationId = table.Column<Guid>(type: "uuid", nullable: false),
                    FloodImpactId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvestigationCommunityImpact", x => new { x.InvestigationId, x.FloodImpactId });
                    table.ForeignKey(
                        name: "FK_InvestigationCommunityImpact_FloodImpacts_FloodImpactId",
                        column: x => x.FloodImpactId,
                        principalSchema: "fortpublic",
                        principalTable: "FloodImpacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InvestigationCommunityImpact_Investigations_InvestigationId",
                        column: x => x.InvestigationId,
                        principalSchema: "fortpublic",
                        principalTable: "Investigations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Relationships between investigation and community flood impacts");

            migrationBuilder.CreateTable(
                name: "InvestigationDestinations",
                schema: "fortpublic",
                columns: table => new
                {
                    InvestigationId = table.Column<Guid>(type: "uuid", nullable: false),
                    FloodProblemId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvestigationDestinations", x => new { x.InvestigationId, x.FloodProblemId });
                    table.ForeignKey(
                        name: "FK_InvestigationDestinations_FloodProblems_FloodProblemId",
                        column: x => x.FloodProblemId,
                        principalSchema: "fortpublic",
                        principalTable: "FloodProblems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InvestigationDestinations_Investigations_InvestigationId",
                        column: x => x.InvestigationId,
                        principalSchema: "fortpublic",
                        principalTable: "Investigations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Relationships between investigation and destination flood problems");

            migrationBuilder.CreateTable(
                name: "InvestigationEntries",
                schema: "fortpublic",
                columns: table => new
                {
                    InvestigationId = table.Column<Guid>(type: "uuid", nullable: false),
                    FloodProblemId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvestigationEntries", x => new { x.InvestigationId, x.FloodProblemId });
                    table.ForeignKey(
                        name: "FK_InvestigationEntries_FloodProblems_FloodProblemId",
                        column: x => x.FloodProblemId,
                        principalSchema: "fortpublic",
                        principalTable: "FloodProblems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InvestigationEntries_Investigations_InvestigationId",
                        column: x => x.InvestigationId,
                        principalSchema: "fortpublic",
                        principalTable: "Investigations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Relationships between investigation and entry flood problems");

            migrationBuilder.CreateTable(
                name: "InvestigationHelpReceived",
                schema: "fortpublic",
                columns: table => new
                {
                    InvestigationId = table.Column<Guid>(type: "uuid", nullable: false),
                    FloodMitigationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvestigationHelpReceived", x => new { x.InvestigationId, x.FloodMitigationId });
                    table.ForeignKey(
                        name: "FK_InvestigationHelpReceived_FloodMitigations_FloodMitigationId",
                        column: x => x.FloodMitigationId,
                        principalSchema: "fortpublic",
                        principalTable: "FloodMitigations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InvestigationHelpReceived_Investigations_InvestigationId",
                        column: x => x.InvestigationId,
                        principalSchema: "fortpublic",
                        principalTable: "Investigations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Relationships between investigation and help received for the flood");

            migrationBuilder.CreateTable(
                name: "InvestigationWarningSources",
                schema: "fortpublic",
                columns: table => new
                {
                    InvestigationId = table.Column<Guid>(type: "uuid", nullable: false),
                    FloodMitigationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvestigationWarningSources", x => new { x.InvestigationId, x.FloodMitigationId });
                    table.ForeignKey(
                        name: "FK_InvestigationWarningSources_FloodMitigations_FloodMitigatio~",
                        column: x => x.FloodMitigationId,
                        principalSchema: "fortpublic",
                        principalTable: "FloodMitigations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InvestigationWarningSources_Investigations_InvestigationId",
                        column: x => x.InvestigationId,
                        principalSchema: "fortpublic",
                        principalTable: "Investigations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Relationships between investigation and water source flood mitigations");

            migrationBuilder.CreateTable(
                name: "ContactRecords",
                schema: "fortpublic",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContactType = table.Column<int>(type: "integer", nullable: false),
                    CreatedUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ContactName = table.Column<string>(type: "text", nullable: false),
                    EmailAddress = table.Column<string>(type: "text", nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    RedactionDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ContactUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    FloodReportId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactRecords", x => x.Id);
                },
                comment: "Contact information for individuals reporting flood incidents and seeking assistance");

            migrationBuilder.CreateTable(
                name: "FloodReports",
                schema: "fortpublic",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Reference = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    CreatedUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    MarkedForDeletionUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    StatusId = table.Column<Guid>(type: "uuid", nullable: false, defaultValue: new Guid("018feb10-38e0-7f30-a546-37ce71f243ae")),
                    EligibilityCheckId = table.Column<Guid>(type: "uuid", nullable: true),
                    InvestigationId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReportOwnerId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReportOwnerAccessUntil = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloodReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FloodReports_ContactRecords_ReportOwnerId",
                        column: x => x.ReportOwnerId,
                        principalSchema: "fortpublic",
                        principalTable: "ContactRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_FloodReports_EligibilityChecks_EligibilityCheckId",
                        column: x => x.EligibilityCheckId,
                        principalSchema: "fortpublic",
                        principalTable: "EligibilityChecks",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FloodReports_Investigations_InvestigationId",
                        column: x => x.InvestigationId,
                        principalSchema: "fortpublic",
                        principalTable: "Investigations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FloodReports_RecordStatuses_StatusId",
                        column: x => x.StatusId,
                        principalSchema: "fortpublic",
                        principalTable: "RecordStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Flood report overviews");

            migrationBuilder.CreateTable(
                name: "FloodReportContactRecords",
                schema: "fortpublic",
                columns: table => new
                {
                    FloodReportId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContactRecordId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloodReportContactRecords", x => new { x.FloodReportId, x.ContactRecordId });
                    table.ForeignKey(
                        name: "FK_FloodReportContactRecord_ContactRecords_ContactRecordId",
                        column: x => x.ContactRecordId,
                        principalSchema: "fortpublic",
                        principalTable: "ContactRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FloodReportContactRecord_FloodReports_FloodReportId",
                        column: x => x.FloodReportId,
                        principalSchema: "fortpublic",
                        principalTable: "FloodReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "fortpublic",
                table: "FloodAuthorities",
                columns: new[] { "Id", "AuthorityDescription", "AuthorityName" },
                values: new object[,]
                {
                    { new Guid("018fd118-9400-76d5-a61a-9ff695c06588"), "Environment Agency", "EA" },
                    { new Guid("018fd119-7e60-7384-bb2b-c157b8b576c6"), "Lead Local Flood Authority", "LLFA" },
                    { new Guid("018fd11a-68c0-7c64-8412-495da1eeb0ba"), "Water Authority", "Water" },
                    { new Guid("018fd11b-5320-7f24-a39d-b76cc8bffc8b"), "Gas Board", "Gas" },
                    { new Guid("018fd11c-3d80-7348-b528-d2cd55312a98"), "Electricity Board", "Electric" },
                    { new Guid("018fd11d-27e0-76bc-9176-177880b52135"), "Emergency responders including resilence direct, government departments and blue light", "CAT Respond" },
                    { new Guid("018fd11e-1240-782f-8aa3-f7a3fea26810"), "Community groups and voluntary sector", "Voluntary" }
                });

            migrationBuilder.InsertData(
                schema: "fortpublic",
                table: "FloodImpacts",
                columns: new[] { "Id", "Category", "CategoryPriority", "OptionOrder", "TypeDescription", "TypeName" },
                values: new object[,]
                {
                    { new Guid("018fd63e-f000-732d-9d84-5f1f4f54f3bd"), "Property Type", null, 1, null, "Residential" },
                    { new Guid("018fd63f-da60-7c6c-9a7c-a197c733e7ea"), "Property Type", null, 2, null, "Commercial" },
                    { new Guid("018fd640-c4c0-7e7c-aa03-d4d09a3e2e80"), "Property Type", null, 3, null, "Other" },
                    { new Guid("018fd641-af20-74f2-9576-38b0dd12f330"), "Property Type", null, 99, null, "Not Specified" },
                    { new Guid("018fd675-de80-7b96-954f-12f13f833dbc"), "Priority", "Internal", 1, null, "Building" },
                    { new Guid("018fd676-c8e0-7d18-b734-63be2020c56c"), "Priority", "External", 2, null, "Grounds" },
                    { new Guid("018fd677-b340-750f-8f91-00a7d5ac4065"), "Priority", "Both", 3, null, "Both" },
                    { new Guid("018fd678-9da0-7703-8109-866e5b539d83"), "Priority", "Other", 4, null, "Unknown" },
                    { new Guid("018fd679-8800-7b74-9606-7e0e238753d5"), "Priority", "Other", 9, null, "Not Specified" },
                    { new Guid("018fd6ac-cd00-7293-abb0-f3d05840e090"), "Zone-R", "Internal", 1, null, "Inside living area" },
                    { new Guid("018fd6ad-b760-79d7-b095-74d9baa9ef5d"), "Zone-R", "Internal", 2, null, "Mobile Home / Caravan" },
                    { new Guid("018fd6ae-a1c0-70f4-97d1-b26b0302d54d"), "Zone-R", "Internal", 3, null, "Basement / Cellar" },
                    { new Guid("018fd6af-8c20-75c0-ba44-eb76e844007a"), "Zone-R", "Internal", 4, null, "Garage attached to property" },
                    { new Guid("018fd6b0-7680-711f-84b9-4d9f961bc82b"), "Zone-R", "Internal", 5, null, "Under floorboards" },
                    { new Guid("018fd6b1-60e0-734c-8bb1-8d483f863cfd"), "Zone-R", "External", 6, null, "Against property wall" },
                    { new Guid("018fd6b2-4b40-7780-8b9a-51ebe0c8d5a6"), "Zone-R", "External", 7, null, "Property Access" },
                    { new Guid("018fd6b3-35a0-7490-896d-02fcfb1709af"), "Zone-R", "External", 8, null, "Outbuilding(s)" },
                    { new Guid("018fd6b4-2000-7a42-921d-63fd1c3c526c"), "Zone-R", "External", 9, null, "Garden" },
                    { new Guid("018fd6b5-0a60-77a4-9d68-9360a287b95f"), "Zone-R", "External", 10, null, "Road" },
                    { new Guid("018fd6e3-bb80-7874-a578-b56a8f6fa390"), "Zone-C", "Internal", 1, null, "Inside building" },
                    { new Guid("018fd6e4-a5e0-7389-aae7-7b5a64b6d35e"), "Zone-C", "Internal", 2, null, "Below ground level floors" },
                    { new Guid("018fd6e5-9040-7a02-a8e2-64d1acd5941d"), "Zone-C", "Internal", 3, null, "Under floorboards" },
                    { new Guid("018fd6e6-7aa0-7a90-ad06-e7a71d76f6dc"), "Zone-C", "External", 4, null, "Against property wall" },
                    { new Guid("018fd6e7-6500-76f2-94b1-e1671217fd29"), "Zone-C", "External", 5, null, "Outbuilding(s)" },
                    { new Guid("018fd6e8-4f60-76cb-a4f3-d83117b1828c"), "Zone-C", "External", 6, null, "Fields / Business Land" },
                    { new Guid("018fd6e9-39c0-7483-a529-7cd3a48fc038"), "Zone-C", "External", 7, null, "Car Park" },
                    { new Guid("018fd6ea-2420-7b30-b20e-5fccfd98b345"), "Zone-C", "External", 8, null, "Access" },
                    { new Guid("018fd6eb-0e80-78a6-b74e-8a65c9293f90"), "Zone-C", "External", 9, null, "Road" },
                    { new Guid("018fd6eb-f8e0-7844-b002-405ef83ab875"), "Zone-C", "Other", 99, null, "Not Sure" },
                    { new Guid("018fd71a-aa00-7ac0-b521-ccf27f194875"), "Service Impact", null, 1, null, "Services not affected" },
                    { new Guid("018fd71b-9460-715b-aa13-d9eabd5b7ef1"), "Service Impact", null, 2, null, "Private Sewer" },
                    { new Guid("018fd71c-7ec0-7a1b-94a6-c7d7ae52b977"), "Service Impact", null, 3, null, "Mains Sewer" },
                    { new Guid("018fd71d-6920-787b-ab3f-b6f251f4834b"), "Service Impact", null, 4, null, "Water Supply" },
                    { new Guid("018fd71e-5380-79a2-8e37-ab4e24f063a2"), "Service Impact", null, 5, null, "Gas" },
                    { new Guid("018fd71f-3de0-7551-b3a4-7916759c83fe"), "Service Impact", null, 6, null, "Electricity" },
                    { new Guid("018fd720-2840-7273-bfcd-4ce03f7f249e"), "Service Impact", null, 7, null, "Phoneline" },
                    { new Guid("018fd721-12a0-7341-a0fb-818543c14e0f"), "Service Impact", null, 99, null, "Not Sure" },
                    { new Guid("018fd751-9880-7fe6-812e-3683961317a9"), "Community Impact", null, 1, null, "All road access blocked" },
                    { new Guid("018fd752-82e0-7560-8b2f-441c7ff1800a"), "Community Impact", null, 2, null, "Some road access blocked" },
                    { new Guid("018fd753-6d40-7327-b7dc-e5286d2a5bf3"), "Community Impact", null, 3, null, "No access to place of work" },
                    { new Guid("018fd754-57a0-7009-b36e-49d223f5515c"), "Community Impact", null, 4, null, "Public transport disrupted" },
                    { new Guid("018fd755-4200-706e-89da-48876a818c73"), "Community Impact", null, 5, null, "Local shop(s) closed" },
                    { new Guid("018fd756-2c60-7616-a03f-6e03f996cd1f"), "Community Impact", null, 99, null, "Not Sure" },
                    { new Guid("018fd788-8700-723b-aa01-d93fa589ab4d"), "Impact Duration", null, 1, null, "Use not disrupted" },
                    { new Guid("018fd789-7160-74da-b17b-871e5de26e3a"), "Impact Duration", null, 2, null, "Up to 1 week" },
                    { new Guid("018fd78a-5bc0-77fe-9930-fe113cc34dc9"), "Impact Duration", null, 3, null, "1 week to 1 month" },
                    { new Guid("018fd78b-4620-72d5-bb2c-6eb8edb20691"), "Impact Duration", null, 4, null, "1 month to 6 months" },
                    { new Guid("018fd78c-3080-7d4e-88ef-4b3013a8bb91"), "Impact Duration", null, 5, null, ">6 months" },
                    { new Guid("018fd78d-1ae0-7fb3-bc3e-a9adc9b3dd7f"), "Impact Duration", null, 6, null, "Still unable" },
                    { new Guid("018fd78e-0540-7b80-ac80-b58c96edc173"), "Impact Duration", null, 99, null, "Not Sure" },
                    { new Guid("018fda40-5400-793d-b3c2-e058c29ef1cb"), "Zone-R", "Other", 99, null, "Not Sure" }
                });

            migrationBuilder.InsertData(
                schema: "fortpublic",
                table: "FloodMitigations",
                columns: new[] { "Id", "Category", "OptionOrder", "TypeDescription", "TypeName" },
                values: new object[,]
                {
                    { new Guid("018fdb65-4c00-7552-bcf3-0a398a590464"), "Action Taken", 99, null, "No Action Taken" },
                    { new Guid("018fdb66-3660-70ed-af58-61a95da37750"), "Action Taken", 2, null, "Sandbags" },
                    { new Guid("018fdb67-20c0-7c09-8aa3-818bc80648f6"), "Action Taken", 3, null, "Sandless Sandbag" },
                    { new Guid("018fdb68-0b20-7840-bbb4-4cc1120720ac"), "Action Taken", 4, null, "Flood Boards / Gate" },
                    { new Guid("018fdb68-f580-761f-8f45-805d18c65823"), "Action Taken", 5, null, "Flood Door" },
                    { new Guid("018fdb69-dfe0-735a-ba5a-389eb2f5f753"), "Action Taken", 6, null, "Back-flow valve" },
                    { new Guid("018fdb6a-ca40-7e06-b6cd-0dab09a39e90"), "Action Taken", 7, null, "Air brick cover" },
                    { new Guid("018fdb6b-b4a0-77c5-9721-e3a1cac011fa"), "Action Taken", 8, null, "Pumped Water" },
                    { new Guid("018fdb6c-9f00-732f-9d67-a087fa117a8a"), "Action Taken", 9, null, "Move Valuables" },
                    { new Guid("018fdb6d-8960-7e00-a446-056d1f74e329"), "Action Taken", 10, null, "Move Car" },
                    { new Guid("018fdb6e-73c0-71c8-a533-0bff3d55eb59"), "Action Taken", 11, null, "Other" },
                    { new Guid("018fdb9c-3a80-7300-8a49-5b3df75adf2a"), "Help Received From", 99, null, "No Help" },
                    { new Guid("018fdb9d-24e0-78ba-9fff-95ac94b38f7c"), "Help Received From", 2, null, "Neighbours / Family" },
                    { new Guid("018fdb9e-0f40-7061-959a-23bfb2bba985"), "Help Received From", 3, null, "Wardens / Volunteers" },
                    { new Guid("018fdb9e-f9a0-70ba-9475-5793cbf66ece"), "Help Received From", 4, null, "Fire and Rescue / Police" },
                    { new Guid("018fdb9f-e400-77bf-868a-633a7e27bc8c"), "Help Received From", 5, null, "Environment Agency" },
                    { new Guid("018fdba0-ce60-77e4-a0a6-91ab51596fad"), "Help Received From", 6, null, "Highways" },
                    { new Guid("018fdba1-b8c0-765c-89f6-bd1d9019db0c"), "Help Received From", 7, null, "Local Authority" },
                    { new Guid("018fdba2-a320-793d-afd9-126986a9a3fb"), "Help Received From", 8, null, "Floodline" },
                    { new Guid("018fdbd3-2900-7fed-9c52-9f4668e28618"), "Warning Source", 99, null, "I did not get a warning" },
                    { new Guid("018fdbd4-1360-7df8-80a2-a8ae26685016"), "Warning Source", 2, null, "Floodline" },
                    { new Guid("018fdbd4-fdc0-71a4-8973-f5efce80c875"), "Warning Source", 3, null, "Television" },
                    { new Guid("018fdbd5-e820-7eb6-bee7-d1de8738d312"), "Warning Source", 4, null, "Radio" },
                    { new Guid("018fdbd6-d280-7d17-9fc4-78608036bd36"), "Warning Source", 5, null, "Social Media/Internet" },
                    { new Guid("018fdbd7-bce0-7545-8aa2-a19a5bda2e83"), "Warning Source", 6, null, "Flood Warden/Volunteer" },
                    { new Guid("018fdbd8-a740-7dbf-9f3b-393e01305c25"), "Warning Source", 7, null, "Neighbours" },
                    { new Guid("018fdbd9-91a0-763d-9773-de4670ae0781"), "Warning Source", 8, null, "Other" },
                    { new Guid("018fdc0a-1780-793d-9239-fe2a17b52571"), "Flood Warden Awareness", 1, null, "Before flooding" },
                    { new Guid("018fdc0b-01e0-78f7-864b-0297b744acad"), "Flood Warden Awareness", 2, null, "During flooding" },
                    { new Guid("018fdc0b-ec40-7769-be33-afc5bf808f01"), "Flood Warden Awareness", 3, null, "After flooding" },
                    { new Guid("018fdc0c-d6a0-73ae-b83c-d13ccfc0da71"), "Flood Warden Awareness", 4, null, "What are flood wardens/volunteers?" }
                });

            migrationBuilder.InsertData(
                schema: "fortpublic",
                table: "FloodProblems",
                columns: new[] { "Id", "Category", "OptionOrder", "TypeDescription", "TypeName" },
                values: new object[,]
                {
                    { new Guid("018fe08b-a800-7cb2-aa1b-f39aefc48152"), "Primary Cause", 1, "Caused by an overflowing main river", "River" },
                    { new Guid("018fe08c-9260-77ad-9e3a-418bb047ff5d"), "Primary Cause", 2, "Caused by an overflowing stream or watercourse (not a main river)", "Stream / Watercourse" },
                    { new Guid("018fe08d-7cc0-70ce-89e3-78360019c9b7"), "Primary Cause", 3, "Caused by an overflowing lake or reservoir", "Lake / Reservoirs" },
                    { new Guid("018fe08e-6720-70ea-9a72-bf9aa0de312b"), "Primary Cause", 4, "Caused by sea water including high tides", "The Sea" },
                    { new Guid("018fe08f-5180-744d-ab20-b58bf6d04fbb"), "Primary Cause", 5, "Caused by blocked ditches or channels", "Ditches and drainage channels" },
                    { new Guid("018fe090-3be0-7d28-b73d-ba671aa61208"), "Primary Cause", 6, "The water is coming out of the ground (groundwater)", "Water rising out of the ground" },
                    { new Guid("018fe091-2640-7a8c-ab1e-5c762268a1d7"), "Primary Cause", 7, "Caused by overwhelmed foul sewer", "Foul drainage (Sewerage)" },
                    { new Guid("018fe092-10a0-7858-8987-1f84395524b7"), "Primary Cause", 8, "Caused by overwhelmed drains (not foul/sewer water)", "Surface water drainage" },
                    { new Guid("018fe092-fb00-7a34-a8c9-bdac73a1acaa"), "Primary Cause", 9, "Caused by blocked road drainage", "Blocked road drainage" },
                    { new Guid("018fe093-e560-74dc-ac18-f4949190dce3"), "Primary Cause", 10, "Caused by an issue with a bridge or underground water channel (culvert)", "Bridge / culvert" },
                    { new Guid("018fe094-cfc0-7156-99f5-2fc20e9e19ea"), "Primary Cause", 11, "Waves caused by vehicles", "Waves caused by vehicles" },
                    { new Guid("018fe095-ba20-7234-b73b-ff8e340dd9fc"), "Primary Cause", 12, "Rainwater flowing over the ground", "Rainwater flowing over the ground" },
                    { new Guid("018fe096-a480-70d8-91f4-03504bcf926c"), "Primary Cause", 99, "I don't know where the water came from", "Not Sure" },
                    { new Guid("018fe0c2-9680-78b5-a5fd-ca2bb2ddd0e3"), "Secondary Cause", 1, "Water flowing from an council maintained road", "Runoff from road" },
                    { new Guid("018fe0c3-80e0-7a22-a8d4-36ad1e5dd626"), "Secondary Cause", 2, "Water flowing from a private road", "Runoff from private road" },
                    { new Guid("018fe0c4-6b40-7441-afbb-381e233f4906"), "Secondary Cause", 3, "Water flowing from a track or footpath", "Runoff from track/path" },
                    { new Guid("018fe0c5-55a0-7991-8ee8-1df41519d18e"), "Secondary Cause", 4, "Water flowing from agricultural land (fields)", "Runoff from agricultural land" },
                    { new Guid("018fe0c6-4000-7e95-84d4-1ad96cf4f598"), "Secondary Cause", 5, "Water flowing from a neighbouring property", "Runoff from other property" },
                    { new Guid("018fe0c7-2a60-7983-b7c3-afa68072aa5f"), "Secondary Cause", 99, "I don't know which option is right", "Not Sure" },
                    { new Guid("018fe0f9-8500-7fd2-8c09-55bcabc21fe8"), "Appearance", 1, "The water was clear / clean", "Clear" },
                    { new Guid("018fe0fa-6f60-7062-a8b6-00a898cc5648"), "Appearance", 2, "The water was muddy / cloudy", "Muddy" },
                    { new Guid("018fe0fb-59c0-7bd1-9634-46897151ff78"), "Appearance", 3, "The water had sewage in it", "Polluted with sewage" },
                    { new Guid("018fe130-7380-7859-b90c-b5b6b347f75c"), "Water Onset", 1, "The water came rapidly (flash flooding)", "Suddenly" },
                    { new Guid("018fe131-5de0-7929-b4f4-dc78a73a4af6"), "Water Onset", 2, "The water rose gradually", "Gradually" },
                    { new Guid("018fe167-6200-7e14-b35b-31be0af65be2"), "Water Speed", 1, "The water was flowing fast", "Fast" },
                    { new Guid("018fe168-4c60-7ba1-90ad-c75fc66ee698"), "Water Speed", 2, "The water was flowing slowly", "Slow (walking pace)" },
                    { new Guid("018fe169-36c0-7e6d-ac4d-83265c6a3fa6"), "Water Speed", 3, "The water was not flowing / still", "Still" },
                    { new Guid("018fe19e-5080-7a35-a80f-ab36da09bba2"), "Duration", 1, "Less than 1 hour", "1" },
                    { new Guid("018fe19f-3ae0-783b-bebb-27c43b4d2df6"), "Duration", 2, "1 hour to 24 hours", "24" },
                    { new Guid("018fe1a0-2540-72a1-8204-316d1500d8bb"), "Duration", 3, "24 hours to 1 week", "168" },
                    { new Guid("018fe1a1-0fa0-73d6-aa9b-495404a336f1"), "Duration", 4, "More than 1 week", "744" },
                    { new Guid("018fe1a1-fa00-76e6-929f-c87588a3853b"), "Duration", 5, "I know how many days/hours", null },
                    { new Guid("018fe1a2-e460-76f0-b29d-663d0a19ddd8"), "Duration", 99, "Not Sure", "48" },
                    { new Guid("018fe1d5-3f00-7ace-aa24-f7337da28d46"), "Water Entry", 1, "The water came through a door", "Door" },
                    { new Guid("018fe1d6-2960-7166-b8bf-729fad31d8dc"), "Water Entry", 2, "The water came through a window", "Windows" },
                    { new Guid("018fe1d7-13c0-7730-8d86-d67c01ec14e5"), "Water Entry", 3, "The water came through an airbrick or vent", "Airbrick" },
                    { new Guid("018fe1d7-fe20-71c0-b136-778d46ed6717"), "Water Entry", 4, "The water came through the walls", "Walls" },
                    { new Guid("018fe1d8-e880-7f2e-9b5e-876bd2770f4a"), "Water Entry", 5, "The water came up through the floor", "Through Floor" },
                    { new Guid("018fe1d9-d2e0-763d-9459-6aa643c1470b"), "Water Entry", 6, "The water came up to the property but did not enter", "External Only" },
                    { new Guid("018fe1da-bd40-7c13-842c-901f01c9158f"), "Water Entry", 7, "None of the options are correct", "Other" },
                    { new Guid("018fe1db-a7a0-7c33-a15d-a4b6fc1ad57e"), "Water Entry", 99, "I don't know which option is right", "Not Sure" },
                    { new Guid("018fe20c-2d80-7e13-add2-2c72f510141d"), "Water Destination", 1, "The flood water was flowing into a main river", "River" },
                    { new Guid("018fe20d-17e0-72fd-a679-ecdfcce68d1a"), "Water Destination", 2, "The flood water was flowing into a stream of watercourse (not a main river)", "Stream / Watercourse" },
                    { new Guid("018fe20e-0240-7a6b-ad35-1e4fc9bcd1c4"), "Water Destination", 3, "The flood water was flowing into the sea", "The Sea" },
                    { new Guid("018fe20e-eca0-744b-95bf-b3a85b0e748b"), "Water Destination", 4, "The flood water was flowing into a ditch or channel", "Ditches and drainage channels" },
                    { new Guid("018fe20f-d700-7097-bdf0-5d88714b5528"), "Water Destination", 5, "The flood water was flowing into road drains", "Road drainage" },
                    { new Guid("018fe210-c160-7359-97f0-fdd430ed229c"), "Water Destination", 99, "I don't know which option is right", "Not Sure" }
                });

            migrationBuilder.InsertData(
                schema: "fortpublic",
                table: "RecordStatuses",
                columns: new[] { "Id", "Category", "Description", "Order", "Text" },
                values: new object[,]
                {
                    { new Guid("018fead8-6000-7481-985a-c1e3c56a48a0"), "General", null, 1, "Yes" },
                    { new Guid("018fead9-4a60-74f3-a824-fe666cd91f99"), "General", null, 2, "No" },
                    { new Guid("018feada-34c0-7e10-a183-7a5161c397dc"), "General", null, 3, "Not Sure" },
                    { new Guid("018feb0f-4e80-767d-8262-36a1217ae690"), "Flood report status", "The record is marked for deletion and will be removed from the system in 48 hours", 99, "Marked for Deletion" },
                    { new Guid("018feb10-38e0-7f30-a546-37ce71f243ae"), "Flood report status", "This is a new record that has not been viewed yet", 1, "New" },
                    { new Guid("018feb11-2340-7449-9a20-83e2043a6817"), "Flood report status", "This record has been viewed but no action has been taken yet", 2, "Viewed" },
                    { new Guid("018feb12-0da0-749b-a59a-cb3ed128d982"), "Flood report status", "Action is needed on this record and needs to be reviewed", 3, "Action needed" },
                    { new Guid("018feb12-f800-75e0-ab95-e780864249c8"), "Flood report status", "Action has been completed on this record", 4, "Action completed" },
                    { new Guid("018feb13-e260-7c11-9106-c179ba7c8ce4"), "Flood report status", "This record has an error and needs to be reviewed", 5, "Error" },
                    { new Guid("018feb46-3d00-72a8-a3e1-056e99014150"), "Area", "A forecast of/imminent risk of buildings/land being flooded", 1, "Prepare Phase" },
                    { new Guid("018feb47-2760-7161-af3d-6c1036e802ed"), "Area", "Buildings/land are currently flooded", 2, "Response Phase" },
                    { new Guid("018feb48-11c0-7fd1-a032-e87cae11748c"), "Area", "A recent flood event where buildings/land are no longer flooded but remedial work to properties is on-going", 3, "Recovery Phase" },
                    { new Guid("018feb48-fc20-7d06-b7cd-b4440af84719"), "Area", "A past flood event where homes/businesses/land are no longer affected", 4, "Analyse Phase" },
                    { new Guid("018feb7d-2b80-7da6-bd00-dd2c83fa2a2e"), "Area Flood Status", null, 1, "Flood Expected: No Flood" },
                    { new Guid("018feb7e-15e0-7a9d-ac21-bc1cc63e081c"), "Area Flood Status", null, 2, "Flood Expected: Help Given" },
                    { new Guid("018feb7f-0040-7d3f-ae3c-796c50850cbe"), "Area Flood Status", null, 3, "Properties Affected" },
                    { new Guid("018feb7f-eaa0-7507-af59-497aac53467d"), "Area Flood Status", null, 4, "Properties Affected: Help Given" },
                    { new Guid("018feb80-d500-7aa7-b155-b6c3218ff2cc"), "Area Flood Status", null, 5, "Buildings Flooded" },
                    { new Guid("018feb81-bf60-788d-86c9-ec5f1cc1871c"), "Area Flood Status", null, 6, "Buildings Flooded: Help Given" },
                    { new Guid("018feb82-a9c0-7d33-bbf1-64422e309748"), "Area Flood Status", null, 7, "No Flooding Occurred" },
                    { new Guid("018febb4-1a00-7aee-9b88-1d748f18c059"), "Validation", null, 1, "Unconfirmed" },
                    { new Guid("018febb5-0460-790e-bd7f-9684e1aa6ce9"), "Validation", null, 2, "Validated" },
                    { new Guid("018febeb-0880-7532-b583-3d9502dffd7b"), "Section19 Status", null, 1, "No Section 19 report" },
                    { new Guid("018febeb-f2e0-7d1d-89a3-b76b1fe98343"), "Section19 Status", null, 2, "Section 19 report required" },
                    { new Guid("018febec-dd40-7863-b45c-e7b9546f588c"), "Section19 Status", null, 3, "Section 19 report in progress" },
                    { new Guid("018febed-c7a0-7994-8c44-4623142fdfb1"), "Section19 Status", null, 4, "Included in Section 19 report" },
                    { new Guid("018fec21-f700-745d-a90f-9f0204c1e2d6"), "Data Protection", null, 1, "Not yet acknowledged" },
                    { new Guid("018fec22-e160-7cda-92ef-1e9b92d7dd1c"), "Data Protection", null, 2, "Agreed" }
                });

            migrationBuilder.InsertData(
                schema: "fortpublic",
                table: "FloodAuthorityFloodProblems",
                columns: new[] { "FloodAuthorityId", "FloodProblemId" },
                values: new object[,]
                {
                    { new Guid("018fd118-9400-76d5-a61a-9ff695c06588"), new Guid("018fe08b-a800-7cb2-aa1b-f39aefc48152") },
                    { new Guid("018fd118-9400-76d5-a61a-9ff695c06588"), new Guid("018fe08c-9260-77ad-9e3a-418bb047ff5d") },
                    { new Guid("018fd118-9400-76d5-a61a-9ff695c06588"), new Guid("018fe08d-7cc0-70ce-89e3-78360019c9b7") },
                    { new Guid("018fd118-9400-76d5-a61a-9ff695c06588"), new Guid("018fe08e-6720-70ea-9a72-bf9aa0de312b") },
                    { new Guid("018fd118-9400-76d5-a61a-9ff695c06588"), new Guid("018fe090-3be0-7d28-b73d-ba671aa61208") },
                    { new Guid("018fd119-7e60-7384-bb2b-c157b8b576c6"), new Guid("018fe08b-a800-7cb2-aa1b-f39aefc48152") },
                    { new Guid("018fd119-7e60-7384-bb2b-c157b8b576c6"), new Guid("018fe08c-9260-77ad-9e3a-418bb047ff5d") },
                    { new Guid("018fd119-7e60-7384-bb2b-c157b8b576c6"), new Guid("018fe08d-7cc0-70ce-89e3-78360019c9b7") },
                    { new Guid("018fd119-7e60-7384-bb2b-c157b8b576c6"), new Guid("018fe08e-6720-70ea-9a72-bf9aa0de312b") },
                    { new Guid("018fd119-7e60-7384-bb2b-c157b8b576c6"), new Guid("018fe08f-5180-744d-ab20-b58bf6d04fbb") },
                    { new Guid("018fd119-7e60-7384-bb2b-c157b8b576c6"), new Guid("018fe090-3be0-7d28-b73d-ba671aa61208") },
                    { new Guid("018fd119-7e60-7384-bb2b-c157b8b576c6"), new Guid("018fe091-2640-7a8c-ab1e-5c762268a1d7") },
                    { new Guid("018fd119-7e60-7384-bb2b-c157b8b576c6"), new Guid("018fe092-10a0-7858-8987-1f84395524b7") },
                    { new Guid("018fd119-7e60-7384-bb2b-c157b8b576c6"), new Guid("018fe092-fb00-7a34-a8c9-bdac73a1acaa") },
                    { new Guid("018fd119-7e60-7384-bb2b-c157b8b576c6"), new Guid("018fe093-e560-74dc-ac18-f4949190dce3") },
                    { new Guid("018fd119-7e60-7384-bb2b-c157b8b576c6"), new Guid("018fe094-cfc0-7156-99f5-2fc20e9e19ea") },
                    { new Guid("018fd119-7e60-7384-bb2b-c157b8b576c6"), new Guid("018fe095-ba20-7234-b73b-ff8e340dd9fc") },
                    { new Guid("018fd119-7e60-7384-bb2b-c157b8b576c6"), new Guid("018fe096-a480-70d8-91f4-03504bcf926c") },
                    { new Guid("018fd119-7e60-7384-bb2b-c157b8b576c6"), new Guid("018fe0c2-9680-78b5-a5fd-ca2bb2ddd0e3") },
                    { new Guid("018fd119-7e60-7384-bb2b-c157b8b576c6"), new Guid("018fe0c3-80e0-7a22-a8d4-36ad1e5dd626") },
                    { new Guid("018fd119-7e60-7384-bb2b-c157b8b576c6"), new Guid("018fe0c4-6b40-7441-afbb-381e233f4906") },
                    { new Guid("018fd119-7e60-7384-bb2b-c157b8b576c6"), new Guid("018fe0c5-55a0-7991-8ee8-1df41519d18e") },
                    { new Guid("018fd119-7e60-7384-bb2b-c157b8b576c6"), new Guid("018fe0c6-4000-7e95-84d4-1ad96cf4f598") },
                    { new Guid("018fd119-7e60-7384-bb2b-c157b8b576c6"), new Guid("018fe0c7-2a60-7983-b7c3-afa68072aa5f") },
                    { new Guid("018fd119-7e60-7384-bb2b-c157b8b576c6"), new Guid("018fe0f9-8500-7fd2-8c09-55bcabc21fe8") },
                    { new Guid("018fd119-7e60-7384-bb2b-c157b8b576c6"), new Guid("018fe0fa-6f60-7062-a8b6-00a898cc5648") },
                    { new Guid("018fd119-7e60-7384-bb2b-c157b8b576c6"), new Guid("018fe0fb-59c0-7bd1-9634-46897151ff78") },
                    { new Guid("018fd119-7e60-7384-bb2b-c157b8b576c6"), new Guid("018fe130-7380-7859-b90c-b5b6b347f75c") },
                    { new Guid("018fd119-7e60-7384-bb2b-c157b8b576c6"), new Guid("018fe131-5de0-7929-b4f4-dc78a73a4af6") },
                    { new Guid("018fd119-7e60-7384-bb2b-c157b8b576c6"), new Guid("018fe167-6200-7e14-b35b-31be0af65be2") },
                    { new Guid("018fd119-7e60-7384-bb2b-c157b8b576c6"), new Guid("018fe168-4c60-7ba1-90ad-c75fc66ee698") },
                    { new Guid("018fd119-7e60-7384-bb2b-c157b8b576c6"), new Guid("018fe169-36c0-7e6d-ac4d-83265c6a3fa6") },
                    { new Guid("018fd119-7e60-7384-bb2b-c157b8b576c6"), new Guid("018fe19e-5080-7a35-a80f-ab36da09bba2") },
                    { new Guid("018fd119-7e60-7384-bb2b-c157b8b576c6"), new Guid("018fe19f-3ae0-783b-bebb-27c43b4d2df6") },
                    { new Guid("018fd119-7e60-7384-bb2b-c157b8b576c6"), new Guid("018fe1a0-2540-72a1-8204-316d1500d8bb") },
                    { new Guid("018fd119-7e60-7384-bb2b-c157b8b576c6"), new Guid("018fe1a1-0fa0-73d6-aa9b-495404a336f1") },
                    { new Guid("018fd119-7e60-7384-bb2b-c157b8b576c6"), new Guid("018fe1a1-fa00-76e6-929f-c87588a3853b") },
                    { new Guid("018fd119-7e60-7384-bb2b-c157b8b576c6"), new Guid("018fe1a2-e460-76f0-b29d-663d0a19ddd8") },
                    { new Guid("018fd119-7e60-7384-bb2b-c157b8b576c6"), new Guid("018fe1d5-3f00-7ace-aa24-f7337da28d46") },
                    { new Guid("018fd119-7e60-7384-bb2b-c157b8b576c6"), new Guid("018fe1d6-2960-7166-b8bf-729fad31d8dc") },
                    { new Guid("018fd119-7e60-7384-bb2b-c157b8b576c6"), new Guid("018fe1d7-13c0-7730-8d86-d67c01ec14e5") },
                    { new Guid("018fd119-7e60-7384-bb2b-c157b8b576c6"), new Guid("018fe1d7-fe20-71c0-b136-778d46ed6717") },
                    { new Guid("018fd119-7e60-7384-bb2b-c157b8b576c6"), new Guid("018fe1d8-e880-7f2e-9b5e-876bd2770f4a") },
                    { new Guid("018fd119-7e60-7384-bb2b-c157b8b576c6"), new Guid("018fe1d9-d2e0-763d-9459-6aa643c1470b") },
                    { new Guid("018fd119-7e60-7384-bb2b-c157b8b576c6"), new Guid("018fe1da-bd40-7c13-842c-901f01c9158f") },
                    { new Guid("018fd119-7e60-7384-bb2b-c157b8b576c6"), new Guid("018fe1db-a7a0-7c33-a15d-a4b6fc1ad57e") },
                    { new Guid("018fd119-7e60-7384-bb2b-c157b8b576c6"), new Guid("018fe20c-2d80-7e13-add2-2c72f510141d") },
                    { new Guid("018fd119-7e60-7384-bb2b-c157b8b576c6"), new Guid("018fe20d-17e0-72fd-a679-ecdfcce68d1a") },
                    { new Guid("018fd119-7e60-7384-bb2b-c157b8b576c6"), new Guid("018fe20e-0240-7a6b-ad35-1e4fc9bcd1c4") },
                    { new Guid("018fd119-7e60-7384-bb2b-c157b8b576c6"), new Guid("018fe20e-eca0-744b-95bf-b3a85b0e748b") },
                    { new Guid("018fd119-7e60-7384-bb2b-c157b8b576c6"), new Guid("018fe20f-d700-7097-bdf0-5d88714b5528") },
                    { new Guid("018fd119-7e60-7384-bb2b-c157b8b576c6"), new Guid("018fe210-c160-7359-97f0-fdd430ed229c") },
                    { new Guid("018fd11a-68c0-7c64-8412-495da1eeb0ba"), new Guid("018fe091-2640-7a8c-ab1e-5c762268a1d7") },
                    { new Guid("018fd11a-68c0-7c64-8412-495da1eeb0ba"), new Guid("018fe092-10a0-7858-8987-1f84395524b7") },
                    { new Guid("018fd11b-5320-7f24-a39d-b76cc8bffc8b"), new Guid("018fe090-3be0-7d28-b73d-ba671aa61208") },
                    { new Guid("018fd11c-3d80-7348-b528-d2cd55312a98"), new Guid("018fe090-3be0-7d28-b73d-ba671aa61208") },
                    { new Guid("018fd11d-27e0-76bc-9176-177880b52135"), new Guid("018fe08b-a800-7cb2-aa1b-f39aefc48152") },
                    { new Guid("018fd11d-27e0-76bc-9176-177880b52135"), new Guid("018fe08d-7cc0-70ce-89e3-78360019c9b7") }
                });

            migrationBuilder.InsertData(
                schema: "fortpublic",
                table: "Organisations",
                columns: new[] { "Id", "DataProtectionStatement", "Description", "EmergencyPlanning", "FloodAuthorityId", "GettingInTouch", "LastUpdatedUtc", "Logo", "Name", "SubmissionReply", "Website" },
                values: new object[,]
                {
                    { new Guid("018fe5b2-0400-7281-b27e-87ba8cbbec3a"), "<p>Dorset County Council is defined as a Lead Local Flood Authority under the Flood and Water Management Act of 2010. We collect data via FORT to inform our Section 19 flood reports and as a starting point for detailed flood investigations and property level grants and protection schemes.</p><p>We may download and keep a copy of personal data submitted via the FORT website to use within our standard working practices.This data will only be used for the management of particular flood reports and personal data will be removed from the systems once there is no longer a need to contact you regarding your submission and once you agree that you no longer wish to provide updates to the record.</p>", "Dorset Council is the Lead Local Flood Authority and we are consulted in relation to planning applications and other statutory duties relating to flooding. We also undertaken Section 19 flood investigations where a flood incident meets our significance criteria.", "Notification sent to the emergency planners who will endeavour to review the record within 2 hours.", new Guid("018fd119-7e60-7384-bb2b-c157b8b576c6"), "If you require further advice please contact Dorset Direct on 01305 221000 and ask to speak to a member of the Flood Risk Management Team.", new DateTimeOffset(new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "https://fort-uk.dorsetcouncil.gov.uk/medialib/orglogo/Dorset_Council_logo_colour.jpg", "Dorset Council", "<p>The Dorset Council flood risk team will be notified of your flood report and will review it in line with their prioritisation criteria.</p><p>The FORT system provides a convenient single location to notify multiple organisations of property level flooding as there is no single body responsible for <a href=\"http://www.local.gov.uk/local-flood-risk-management/-/journal_content/56/10180/3572186/ARTICLE\" target=\"_blank\">managing flood risk in the UK</a>.The FORT system aims to ensure that reports of flooding are passed on to the correct organisation(s).</p><p>The management of any further actions will be from the organisation(s) receiving the record. It is important to realise that each recipient will apply their own plans and policies to the records received and prioritise reports accordingly. You may not receive a response and your record will not trigger any emergency action(s). The level of feedback added to your record, by partner organisations using the FORT system, shall again depend upon their own policies.</p>", "https://www.dorsetcouncil.gov.uk/" },
                    { new Guid("018fe5b2-ee60-7347-aa68-16b8f528dab0"), null, "The Environment Agency is a Risk Authority and we are consulted in relation to planning applications and other statutory duties relating to flooding from main rivers and the sea.", null, new Guid("018fd118-9400-76d5-a61a-9ff695c06588"), null, new DateTimeOffset(new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "https://assets.publishing.service.gov.uk/government/uploads/system/uploads/organisation/logo/199/environment-agency-logo-480w.png", "Environment Agency (Wessex)", null, "https://www.gov.uk/government/organisations/environment-agency" },
                    { new Guid("018fe5b3-d8c0-7bc1-95fa-7dcc50ee866d"), null, "The Environment Agency is a Risk Authority and we are consulted in relation to planning applications and other statutory duties relating to flooding from main rivers and the sea.", null, new Guid("018fd118-9400-76d5-a61a-9ff695c06588"), null, new DateTimeOffset(new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "https://assets.publishing.service.gov.uk/government/uploads/system/uploads/organisation/logo/199/environment-agency-logo-480w.png", "Environment Agency (Devon & Cornwall)", null, "https://www.gov.uk/government/organisations/environment-agency" }
                });

            migrationBuilder.InsertData(
                schema: "fortpublic",
                table: "FloodResponsibilities",
                columns: new[] { "AdminUnitId", "OrganisationId", "Description", "LookupDate", "Name" },
                values: new object[,]
                {
                    { 175926, new Guid("018fe5b2-0400-7281-b27e-87ba8cbbec3a"), "Unitary Authority", new DateOnly(2024, 6, 24), "Bournemouth, Christchurch and Poole" },
                    { 175988, new Guid("018fe5b2-0400-7281-b27e-87ba8cbbec3a"), "Unitary Authority", new DateOnly(2024, 6, 24), "Dorset" },
                    { 22713, new Guid("018fe5b3-d8c0-7bc1-95fa-7dcc50ee866d"), "District", new DateOnly(2024, 6, 24), "East Devon District" },
                    { 22767, new Guid("018fe5b3-d8c0-7bc1-95fa-7dcc50ee866d"), "District", new DateOnly(2024, 6, 24), "West Devon District (B)" },
                    { 22902, new Guid("018fe5b3-d8c0-7bc1-95fa-7dcc50ee866d"), "District", new DateOnly(2024, 6, 24), "Mid Devon District" },
                    { 22933, new Guid("018fe5b3-d8c0-7bc1-95fa-7dcc50ee866d"), "District", new DateOnly(2024, 6, 24), "North Devon District" },
                    { 23147, new Guid("018fe5b3-d8c0-7bc1-95fa-7dcc50ee866d"), "County", new DateOnly(2024, 6, 24), "Devon County" },
                    { 43750, new Guid("018fe5b3-d8c0-7bc1-95fa-7dcc50ee866d"), "Unitary Authority", new DateOnly(2024, 6, 24), "Cornwall" },
                    { 175926, new Guid("018fe5b3-d8c0-7bc1-95fa-7dcc50ee866d"), "Unitary Authority", new DateOnly(2024, 6, 24), "Bournemouth, Christchurch and Poole" },
                    { 175988, new Guid("018fe5b3-d8c0-7bc1-95fa-7dcc50ee866d"), "Unitary Authority", new DateOnly(2024, 6, 24), "Dorset" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContactRecords_FloodReportId_UniqueWhenNoUser",
                schema: "fortpublic",
                table: "ContactRecords",
                column: "FloodReportId",
                unique: true,
                filter: "\"ContactUserId\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EligibilityCheckCommercials_FloodImpactId",
                schema: "fortpublic",
                table: "EligibilityCheckCommercials",
                column: "FloodImpactId");

            migrationBuilder.CreateIndex(
                name: "IX_EligibilityCheckResidentials_FloodImpactId",
                schema: "fortpublic",
                table: "EligibilityCheckResidentials",
                column: "FloodImpactId");

            migrationBuilder.CreateIndex(
                name: "IX_EligibilityCheckRunoffSource_FloodProblemId",
                schema: "fortpublic",
                table: "EligibilityCheckRunoffSource",
                column: "FloodProblemId");

            migrationBuilder.CreateIndex(
                name: "IX_EligibilityChecks_VulnerablePeopleId",
                schema: "fortpublic",
                table: "EligibilityChecks",
                column: "VulnerablePeopleId");

            migrationBuilder.CreateIndex(
                name: "IX_EligibilityCheckSources_FloodProblemId",
                schema: "fortpublic",
                table: "EligibilityCheckSources",
                column: "FloodProblemId");

            migrationBuilder.CreateIndex(
                name: "IX_FloodAuthorityFloodProblems_FloodProblemId",
                schema: "fortpublic",
                table: "FloodAuthorityFloodProblems",
                column: "FloodProblemId");

            migrationBuilder.CreateIndex(
                name: "IX_FloodImpacts_Category",
                schema: "fortpublic",
                table: "FloodImpacts",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_FloodProblems_Category",
                schema: "fortpublic",
                table: "FloodProblems",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_FloodReportContactRecords_ContactRecordId",
                schema: "fortpublic",
                table: "FloodReportContactRecords",
                column: "ContactRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_FloodReports_EligibilityCheckId",
                schema: "fortpublic",
                table: "FloodReports",
                column: "EligibilityCheckId");

            migrationBuilder.CreateIndex(
                name: "IX_FloodReports_Id",
                schema: "fortpublic",
                table: "FloodReports",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FloodReports_InvestigationId",
                schema: "fortpublic",
                table: "FloodReports",
                column: "InvestigationId");

            migrationBuilder.CreateIndex(
                name: "IX_FloodReports_Reference",
                schema: "fortpublic",
                table: "FloodReports",
                column: "Reference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FloodReports_ReportOwnerId",
                schema: "fortpublic",
                table: "FloodReports",
                column: "ReportOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_FloodReports_StatusId",
                schema: "fortpublic",
                table: "FloodReports",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_InboxState_Delivered",
                schema: "fortpublic",
                table: "InboxState",
                column: "Delivered");

            migrationBuilder.CreateIndex(
                name: "IX_InvestigationActionsTaken_FloodMitigationId",
                schema: "fortpublic",
                table: "InvestigationActionsTaken",
                column: "FloodMitigationId");

            migrationBuilder.CreateIndex(
                name: "IX_InvestigationCommunityImpact_FloodImpactId",
                schema: "fortpublic",
                table: "InvestigationCommunityImpact",
                column: "FloodImpactId");

            migrationBuilder.CreateIndex(
                name: "IX_InvestigationDestinations_FloodProblemId",
                schema: "fortpublic",
                table: "InvestigationDestinations",
                column: "FloodProblemId");

            migrationBuilder.CreateIndex(
                name: "IX_InvestigationEntries_FloodProblemId",
                schema: "fortpublic",
                table: "InvestigationEntries",
                column: "FloodProblemId");

            migrationBuilder.CreateIndex(
                name: "IX_InvestigationHelpReceived_FloodMitigationId",
                schema: "fortpublic",
                table: "InvestigationHelpReceived",
                column: "FloodMitigationId");

            migrationBuilder.CreateIndex(
                name: "IX_Investigations_AppearanceId",
                schema: "fortpublic",
                table: "Investigations",
                column: "AppearanceId");

            migrationBuilder.CreateIndex(
                name: "IX_Investigations_BeginId",
                schema: "fortpublic",
                table: "Investigations",
                column: "BeginId");

            migrationBuilder.CreateIndex(
                name: "IX_Investigations_FloodlineId",
                schema: "fortpublic",
                table: "Investigations",
                column: "FloodlineId");

            migrationBuilder.CreateIndex(
                name: "IX_Investigations_HistoryOfFloodingId",
                schema: "fortpublic",
                table: "Investigations",
                column: "HistoryOfFloodingId");

            migrationBuilder.CreateIndex(
                name: "IX_Investigations_IsPeakDepthKnownId",
                schema: "fortpublic",
                table: "Investigations",
                column: "IsPeakDepthKnownId");

            migrationBuilder.CreateIndex(
                name: "IX_Investigations_WarningAppropriateId",
                schema: "fortpublic",
                table: "Investigations",
                column: "WarningAppropriateId");

            migrationBuilder.CreateIndex(
                name: "IX_Investigations_WarningReceivedId",
                schema: "fortpublic",
                table: "Investigations",
                column: "WarningReceivedId");

            migrationBuilder.CreateIndex(
                name: "IX_Investigations_WarningTimelyId",
                schema: "fortpublic",
                table: "Investigations",
                column: "WarningTimelyId");

            migrationBuilder.CreateIndex(
                name: "IX_Investigations_WaterSpeedId",
                schema: "fortpublic",
                table: "Investigations",
                column: "WaterSpeedId");

            migrationBuilder.CreateIndex(
                name: "IX_Investigations_WereVehiclesDamagedId",
                schema: "fortpublic",
                table: "Investigations",
                column: "WereVehiclesDamagedId");

            migrationBuilder.CreateIndex(
                name: "IX_Investigations_WhenWaterEnteredKnownId",
                schema: "fortpublic",
                table: "Investigations",
                column: "WhenWaterEnteredKnownId");

            migrationBuilder.CreateIndex(
                name: "IX_InvestigationWarningSources_FloodMitigationId",
                schema: "fortpublic",
                table: "InvestigationWarningSources",
                column: "FloodMitigationId");

            migrationBuilder.CreateIndex(
                name: "IX_Organisations_FloodAuthorityId",
                schema: "fortpublic",
                table: "Organisations",
                column: "FloodAuthorityId");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_EnqueueTime",
                schema: "fortpublic",
                table: "OutboxMessage",
                column: "EnqueueTime");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_ExpirationTime",
                schema: "fortpublic",
                table: "OutboxMessage",
                column: "ExpirationTime");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_InboxMessageId_InboxConsumerId_SequenceNumber",
                schema: "fortpublic",
                table: "OutboxMessage",
                columns: new[] { "InboxMessageId", "InboxConsumerId", "SequenceNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_OutboxId_SequenceNumber",
                schema: "fortpublic",
                table: "OutboxMessage",
                columns: new[] { "OutboxId", "SequenceNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OutboxState_Created",
                schema: "fortpublic",
                table: "OutboxState",
                column: "Created");

            migrationBuilder.CreateIndex(
                name: "IX_RecordStatuses_Category",
                schema: "fortpublic",
                table: "RecordStatuses",
                column: "Category");

            migrationBuilder.AddForeignKey(
                name: "FK_ContactRecords_FloodReports_FloodReportId",
                schema: "fortpublic",
                table: "ContactRecords",
                column: "FloodReportId",
                principalSchema: "fortpublic",
                principalTable: "FloodReports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContactRecords_FloodReports_FloodReportId",
                schema: "fortpublic",
                table: "ContactRecords");

            migrationBuilder.DropTable(
                name: "EligibilityCheckCommercials",
                schema: "fortpublic");

            migrationBuilder.DropTable(
                name: "EligibilityCheckResidentials",
                schema: "fortpublic");

            migrationBuilder.DropTable(
                name: "EligibilityCheckRunoffSource",
                schema: "fortpublic");

            migrationBuilder.DropTable(
                name: "EligibilityCheckSources",
                schema: "fortpublic");

            migrationBuilder.DropTable(
                name: "FloodAuthorityFloodProblems",
                schema: "fortpublic");

            migrationBuilder.DropTable(
                name: "FloodReportContactRecords",
                schema: "fortpublic");

            migrationBuilder.DropTable(
                name: "FloodResponsibilities",
                schema: "fortpublic");

            migrationBuilder.DropTable(
                name: "InvestigationActionsTaken",
                schema: "fortpublic");

            migrationBuilder.DropTable(
                name: "InvestigationCommunityImpact",
                schema: "fortpublic");

            migrationBuilder.DropTable(
                name: "InvestigationDestinations",
                schema: "fortpublic");

            migrationBuilder.DropTable(
                name: "InvestigationEntries",
                schema: "fortpublic");

            migrationBuilder.DropTable(
                name: "InvestigationHelpReceived",
                schema: "fortpublic");

            migrationBuilder.DropTable(
                name: "InvestigationWarningSources",
                schema: "fortpublic");

            migrationBuilder.DropTable(
                name: "OutboxMessage",
                schema: "fortpublic");

            migrationBuilder.DropTable(
                name: "Organisations",
                schema: "fortpublic");

            migrationBuilder.DropTable(
                name: "FloodImpacts",
                schema: "fortpublic");

            migrationBuilder.DropTable(
                name: "FloodMitigations",
                schema: "fortpublic");

            migrationBuilder.DropTable(
                name: "InboxState",
                schema: "fortpublic");

            migrationBuilder.DropTable(
                name: "OutboxState",
                schema: "fortpublic");

            migrationBuilder.DropTable(
                name: "FloodAuthorities",
                schema: "fortpublic");

            migrationBuilder.DropTable(
                name: "FloodReports",
                schema: "fortpublic");

            migrationBuilder.DropTable(
                name: "ContactRecords",
                schema: "fortpublic");

            migrationBuilder.DropTable(
                name: "EligibilityChecks",
                schema: "fortpublic");

            migrationBuilder.DropTable(
                name: "Investigations",
                schema: "fortpublic");

            migrationBuilder.DropTable(
                name: "FloodProblems",
                schema: "fortpublic");

            migrationBuilder.DropTable(
                name: "RecordStatuses",
                schema: "fortpublic");
        }
    }
}

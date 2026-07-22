using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FloodOnlineReportingTool.Database.Migrations
{
    /// <inheritdoc />
    public partial class ImportTablesAndSeedValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FailedImports",
                schema: "fortpublic",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Reference = table.Column<string>(type: "text", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FailedImports", x => x.Id);
                });

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd63e-f000-732d-9d84-5f1f4f54f3bd"),
                column: "TypeDescription",
                value: "Residential");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd63f-da60-7c6c-9a7c-a197c733e7ea"),
                column: "TypeDescription",
                value: "Commercial");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd640-c4c0-7e7c-aa03-d4d09a3e2e80"),
                column: "TypeDescription",
                value: "Other");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd641-af20-74f2-9576-38b0dd12f330"),
                column: "TypeDescription",
                value: "Not Specified");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd675-de80-7b96-954f-12f13f833dbc"),
                column: "TypeDescription",
                value: "Building");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd676-c8e0-7d18-b734-63be2020c56c"),
                column: "TypeDescription",
                value: "Grounds");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd677-b340-750f-8f91-00a7d5ac4065"),
                column: "TypeDescription",
                value: "Both");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd678-9da0-7703-8109-866e5b539d83"),
                column: "TypeDescription",
                value: "Unknown");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd679-8800-7b74-9606-7e0e238753d5"),
                column: "TypeDescription",
                value: "Not Specified");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd6ac-cd00-7293-abb0-f3d05840e090"),
                column: "TypeDescription",
                value: "Inside living area");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd6ad-b760-79d7-b095-74d9baa9ef5d"),
                column: "TypeDescription",
                value: "Mobile Home / Caravan");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd6ae-a1c0-70f4-97d1-b26b0302d54d"),
                column: "TypeDescription",
                value: "Basement / Cellar");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd6af-8c20-75c0-ba44-eb76e844007a"),
                column: "TypeDescription",
                value: "Garage attached to property");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd6b0-7680-711f-84b9-4d9f961bc82b"),
                column: "TypeDescription",
                value: "Under floorboards");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd6b1-60e0-734c-8bb1-8d483f863cfd"),
                column: "TypeDescription",
                value: "Against property wall");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd6b2-4b40-7780-8b9a-51ebe0c8d5a6"),
                column: "TypeDescription",
                value: "Property Access");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd6b3-35a0-7490-896d-02fcfb1709af"),
                column: "TypeDescription",
                value: "Outbuilding(s)");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd6b4-2000-7a42-921d-63fd1c3c526c"),
                column: "TypeDescription",
                value: "Garden");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd6b5-0a60-77a4-9d68-9360a287b95f"),
                column: "TypeDescription",
                value: "Road");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd6e3-bb80-7874-a578-b56a8f6fa390"),
                column: "TypeDescription",
                value: "Inside building");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd6e4-a5e0-7389-aae7-7b5a64b6d35e"),
                column: "TypeDescription",
                value: "Below ground level floors");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd6e5-9040-7a02-a8e2-64d1acd5941d"),
                column: "TypeDescription",
                value: "Under floorboards");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd6e6-7aa0-7a90-ad06-e7a71d76f6dc"),
                column: "TypeDescription",
                value: "Against property wall");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd6e7-6500-76f2-94b1-e1671217fd29"),
                column: "TypeDescription",
                value: "Outbuilding(s)");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd6e8-4f60-76cb-a4f3-d83117b1828c"),
                column: "TypeDescription",
                value: "Fields / Business Land");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd6e9-39c0-7483-a529-7cd3a48fc038"),
                column: "TypeDescription",
                value: "Car Park");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd6ea-2420-7b30-b20e-5fccfd98b345"),
                column: "TypeDescription",
                value: "Access");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd6eb-0e80-78a6-b74e-8a65c9293f90"),
                column: "TypeDescription",
                value: "Road");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd6eb-f8e0-7844-b002-405ef83ab875"),
                column: "TypeDescription",
                value: "Not Sure");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd71a-aa00-7ac0-b521-ccf27f194875"),
                column: "TypeDescription",
                value: "Services not affected");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd71b-9460-715b-aa13-d9eabd5b7ef1"),
                column: "TypeDescription",
                value: "Private sewer");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd71c-7ec0-7a1b-94a6-c7d7ae52b977"),
                column: "TypeDescription",
                value: "Mains sewer");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd71d-6920-787b-ab3f-b6f251f4834b"),
                column: "TypeDescription",
                value: "Water supply");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd71e-5380-79a2-8e37-ab4e24f063a2"),
                column: "TypeDescription",
                value: "Gas");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd71f-3de0-7551-b3a4-7916759c83fe"),
                column: "TypeDescription",
                value: "Electricity");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd720-2840-7273-bfcd-4ce03f7f249e"),
                column: "TypeDescription",
                value: "Phoneline");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd721-12a0-7341-a0fb-818543c14e0f"),
                column: "TypeDescription",
                value: "Not sure");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd751-9880-7fe6-812e-3683961317a9"),
                column: "TypeDescription",
                value: "All road access blocked");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd752-82e0-7560-8b2f-441c7ff1800a"),
                column: "TypeDescription",
                value: "Some road access blocked");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd753-6d40-7327-b7dc-e5286d2a5bf3"),
                column: "TypeDescription",
                value: "No access to place of work");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd754-57a0-7009-b36e-49d223f5515c"),
                column: "TypeDescription",
                value: "Public transport disrupted");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd755-4200-706e-89da-48876a818c73"),
                column: "TypeDescription",
                value: "Local shop(s) closed");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd756-2c60-7616-a03f-6e03f996cd1f"),
                column: "TypeDescription",
                value: "Not Sure");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd788-8700-723b-aa01-d93fa589ab4d"),
                column: "TypeDescription",
                value: "Use not disrupted");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd789-7160-74da-b17b-871e5de26e3a"),
                column: "TypeDescription",
                value: "Up to 1 week");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd78a-5bc0-77fe-9930-fe113cc34dc9"),
                column: "TypeDescription",
                value: "1 week to 1 month");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd78b-4620-72d5-bb2c-6eb8edb20691"),
                column: "TypeDescription",
                value: "1 month to 6 months");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd78c-3080-7d4e-88ef-4b3013a8bb91"),
                column: "TypeDescription",
                value: ">6 months");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd78d-1ae0-7fb3-bc3e-a9adc9b3dd7f"),
                column: "TypeDescription",
                value: "Still unable");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd78e-0540-7b80-ac80-b58c96edc173"),
                column: "TypeDescription",
                value: "Not Sure");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fda40-5400-793d-b3c2-e058c29ef1cb"),
                column: "TypeDescription",
                value: "Not Sure");

            migrationBuilder.InsertData(
                schema: "fortpublic",
                table: "FloodImpacts",
                columns: new[] { "Id", "Category", "CategoryPriority", "OptionOrder", "TypeDescription", "TypeName" },
                values: new object[] { new Guid("019f6a3a-f7eb-74ef-824d-5b673146160e"), "All", null, 99, "Imported record with unknown value", "Unknown" });

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodMitigations",
                keyColumn: "Id",
                keyValue: new Guid("018fdb65-4c00-7552-bcf3-0a398a590464"),
                column: "TypeDescription",
                value: "No Action Taken");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodMitigations",
                keyColumn: "Id",
                keyValue: new Guid("018fdb66-3660-70ed-af58-61a95da37750"),
                column: "TypeDescription",
                value: "Sandbags");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodMitigations",
                keyColumn: "Id",
                keyValue: new Guid("018fdb67-20c0-7c09-8aa3-818bc80648f6"),
                column: "TypeDescription",
                value: "Sandless Sandbag");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodMitigations",
                keyColumn: "Id",
                keyValue: new Guid("018fdb68-0b20-7840-bbb4-4cc1120720ac"),
                column: "TypeDescription",
                value: "Flood Boards / Gate");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodMitigations",
                keyColumn: "Id",
                keyValue: new Guid("018fdb68-f580-761f-8f45-805d18c65823"),
                column: "TypeDescription",
                value: "Flood Door");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodMitigations",
                keyColumn: "Id",
                keyValue: new Guid("018fdb69-dfe0-735a-ba5a-389eb2f5f753"),
                column: "TypeDescription",
                value: "Back-flow valve");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodMitigations",
                keyColumn: "Id",
                keyValue: new Guid("018fdb6a-ca40-7e06-b6cd-0dab09a39e90"),
                column: "TypeDescription",
                value: "Air brick cover");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodMitigations",
                keyColumn: "Id",
                keyValue: new Guid("018fdb6b-b4a0-77c5-9721-e3a1cac011fa"),
                column: "TypeDescription",
                value: "Pumped Water");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodMitigations",
                keyColumn: "Id",
                keyValue: new Guid("018fdb6c-9f00-732f-9d67-a087fa117a8a"),
                column: "TypeDescription",
                value: "Move Valuables");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodMitigations",
                keyColumn: "Id",
                keyValue: new Guid("018fdb6d-8960-7e00-a446-056d1f74e329"),
                column: "TypeDescription",
                value: "Move Car");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodMitigations",
                keyColumn: "Id",
                keyValue: new Guid("018fdb6e-73c0-71c8-a533-0bff3d55eb59"),
                column: "TypeDescription",
                value: "Other");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodMitigations",
                keyColumn: "Id",
                keyValue: new Guid("018fdb9c-3a80-7300-8a49-5b3df75adf2a"),
                column: "TypeDescription",
                value: "No Help");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodMitigations",
                keyColumn: "Id",
                keyValue: new Guid("018fdb9d-24e0-78ba-9fff-95ac94b38f7c"),
                column: "TypeDescription",
                value: "Neighbours / Family");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodMitigations",
                keyColumn: "Id",
                keyValue: new Guid("018fdb9e-0f40-7061-959a-23bfb2bba985"),
                column: "TypeDescription",
                value: "Wardens / Volunteers");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodMitigations",
                keyColumn: "Id",
                keyValue: new Guid("018fdb9e-f9a0-70ba-9475-5793cbf66ece"),
                column: "TypeDescription",
                value: "Fire and Rescue / Police");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodMitigations",
                keyColumn: "Id",
                keyValue: new Guid("018fdb9f-e400-77bf-868a-633a7e27bc8c"),
                column: "TypeDescription",
                value: "Environment Agency");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodMitigations",
                keyColumn: "Id",
                keyValue: new Guid("018fdba0-ce60-77e4-a0a6-91ab51596fad"),
                column: "TypeDescription",
                value: "Highways");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodMitigations",
                keyColumn: "Id",
                keyValue: new Guid("018fdba1-b8c0-765c-89f6-bd1d9019db0c"),
                column: "TypeDescription",
                value: "Local Authority");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodMitigations",
                keyColumn: "Id",
                keyValue: new Guid("018fdba2-a320-793d-afd9-126986a9a3fb"),
                column: "TypeDescription",
                value: "Floodline");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodMitigations",
                keyColumn: "Id",
                keyValue: new Guid("018fdbd3-2900-7fed-9c52-9f4668e28618"),
                column: "TypeDescription",
                value: "I did not get a warning");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodMitigations",
                keyColumn: "Id",
                keyValue: new Guid("018fdbd4-1360-7df8-80a2-a8ae26685016"),
                column: "TypeDescription",
                value: "Floodline");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodMitigations",
                keyColumn: "Id",
                keyValue: new Guid("018fdbd4-fdc0-71a4-8973-f5efce80c875"),
                column: "TypeDescription",
                value: "Television");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodMitigations",
                keyColumn: "Id",
                keyValue: new Guid("018fdbd5-e820-7eb6-bee7-d1de8738d312"),
                column: "TypeDescription",
                value: "Radio");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodMitigations",
                keyColumn: "Id",
                keyValue: new Guid("018fdbd6-d280-7d17-9fc4-78608036bd36"),
                column: "TypeDescription",
                value: "Social Media/Internet");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodMitigations",
                keyColumn: "Id",
                keyValue: new Guid("018fdbd7-bce0-7545-8aa2-a19a5bda2e83"),
                column: "TypeDescription",
                value: "Flood Warden/Volunteer");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodMitigations",
                keyColumn: "Id",
                keyValue: new Guid("018fdbd8-a740-7dbf-9f3b-393e01305c25"),
                column: "TypeDescription",
                value: "Neighbours");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodMitigations",
                keyColumn: "Id",
                keyValue: new Guid("018fdbd9-91a0-763d-9773-de4670ae0781"),
                column: "TypeDescription",
                value: "Other");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodMitigations",
                keyColumn: "Id",
                keyValue: new Guid("018fdc0a-1780-793d-9239-fe2a17b52571"),
                column: "TypeDescription",
                value: "Before flooding");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodMitigations",
                keyColumn: "Id",
                keyValue: new Guid("018fdc0b-01e0-78f7-864b-0297b744acad"),
                column: "TypeDescription",
                value: "During flooding");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodMitigations",
                keyColumn: "Id",
                keyValue: new Guid("018fdc0b-ec40-7769-be33-afc5bf808f01"),
                column: "TypeDescription",
                value: "After flooding");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodMitigations",
                keyColumn: "Id",
                keyValue: new Guid("018fdc0c-d6a0-73ae-b83c-d13ccfc0da71"),
                column: "TypeDescription",
                value: "What are flood wardens/volunteers?");

            migrationBuilder.InsertData(
                schema: "fortpublic",
                table: "FloodMitigations",
                columns: new[] { "Id", "Category", "OptionOrder", "TypeDescription", "TypeName" },
                values: new object[] { new Guid("019f6a3a-f7eb-7c43-97d8-2c7cfa9d79e3"), "All", 99, "Imported record with unknown value", "Unknown" });

            migrationBuilder.InsertData(
                schema: "fortpublic",
                table: "FloodProblems",
                columns: new[] { "Id", "Category", "OptionOrder", "TypeDescription", "TypeName" },
                values: new object[] { new Guid("019f6a3a-f7eb-748d-a169-a057be99f012"), "All", 99, "Imported record with unknown value", "Unknown" });

            migrationBuilder.InsertData(
                schema: "fortpublic",
                table: "FloodAuthorityFloodProblems",
                columns: new[] { "FloodAuthorityId", "FloodProblemId" },
                values: new object[] { new Guid("018fd119-7e60-7384-bb2b-c157b8b576c6"), new Guid("019f6a3a-f7eb-748d-a169-a057be99f012") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FailedImports",
                schema: "fortpublic");

            migrationBuilder.DeleteData(
                schema: "fortpublic",
                table: "FloodAuthorityFloodProblems",
                keyColumns: new[] { "FloodAuthorityId", "FloodProblemId" },
                keyValues: new object[] { new Guid("018fd119-7e60-7384-bb2b-c157b8b576c6"), new Guid("019f6a3a-f7eb-748d-a169-a057be99f012") });

            migrationBuilder.DeleteData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("019f6a3a-f7eb-74ef-824d-5b673146160e"));

            migrationBuilder.DeleteData(
                schema: "fortpublic",
                table: "FloodMitigations",
                keyColumn: "Id",
                keyValue: new Guid("019f6a3a-f7eb-7c43-97d8-2c7cfa9d79e3"));

            migrationBuilder.DeleteData(
                schema: "fortpublic",
                table: "FloodProblems",
                keyColumn: "Id",
                keyValue: new Guid("019f6a3a-f7eb-748d-a169-a057be99f012"));

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd63e-f000-732d-9d84-5f1f4f54f3bd"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd63f-da60-7c6c-9a7c-a197c733e7ea"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd640-c4c0-7e7c-aa03-d4d09a3e2e80"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd641-af20-74f2-9576-38b0dd12f330"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd675-de80-7b96-954f-12f13f833dbc"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd676-c8e0-7d18-b734-63be2020c56c"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd677-b340-750f-8f91-00a7d5ac4065"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd678-9da0-7703-8109-866e5b539d83"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd679-8800-7b74-9606-7e0e238753d5"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd6ac-cd00-7293-abb0-f3d05840e090"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd6ad-b760-79d7-b095-74d9baa9ef5d"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd6ae-a1c0-70f4-97d1-b26b0302d54d"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd6af-8c20-75c0-ba44-eb76e844007a"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd6b0-7680-711f-84b9-4d9f961bc82b"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd6b1-60e0-734c-8bb1-8d483f863cfd"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd6b2-4b40-7780-8b9a-51ebe0c8d5a6"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd6b3-35a0-7490-896d-02fcfb1709af"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd6b4-2000-7a42-921d-63fd1c3c526c"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd6b5-0a60-77a4-9d68-9360a287b95f"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd6e3-bb80-7874-a578-b56a8f6fa390"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd6e4-a5e0-7389-aae7-7b5a64b6d35e"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd6e5-9040-7a02-a8e2-64d1acd5941d"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd6e6-7aa0-7a90-ad06-e7a71d76f6dc"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd6e7-6500-76f2-94b1-e1671217fd29"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd6e8-4f60-76cb-a4f3-d83117b1828c"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd6e9-39c0-7483-a529-7cd3a48fc038"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd6ea-2420-7b30-b20e-5fccfd98b345"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd6eb-0e80-78a6-b74e-8a65c9293f90"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd6eb-f8e0-7844-b002-405ef83ab875"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd71a-aa00-7ac0-b521-ccf27f194875"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd71b-9460-715b-aa13-d9eabd5b7ef1"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd71c-7ec0-7a1b-94a6-c7d7ae52b977"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd71d-6920-787b-ab3f-b6f251f4834b"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd71e-5380-79a2-8e37-ab4e24f063a2"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd71f-3de0-7551-b3a4-7916759c83fe"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd720-2840-7273-bfcd-4ce03f7f249e"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd721-12a0-7341-a0fb-818543c14e0f"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd751-9880-7fe6-812e-3683961317a9"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd752-82e0-7560-8b2f-441c7ff1800a"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd753-6d40-7327-b7dc-e5286d2a5bf3"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd754-57a0-7009-b36e-49d223f5515c"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd755-4200-706e-89da-48876a818c73"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd756-2c60-7616-a03f-6e03f996cd1f"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd788-8700-723b-aa01-d93fa589ab4d"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd789-7160-74da-b17b-871e5de26e3a"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd78a-5bc0-77fe-9930-fe113cc34dc9"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd78b-4620-72d5-bb2c-6eb8edb20691"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd78c-3080-7d4e-88ef-4b3013a8bb91"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd78d-1ae0-7fb3-bc3e-a9adc9b3dd7f"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd78e-0540-7b80-ac80-b58c96edc173"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fda40-5400-793d-b3c2-e058c29ef1cb"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodMitigations",
                keyColumn: "Id",
                keyValue: new Guid("018fdb65-4c00-7552-bcf3-0a398a590464"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodMitigations",
                keyColumn: "Id",
                keyValue: new Guid("018fdb66-3660-70ed-af58-61a95da37750"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodMitigations",
                keyColumn: "Id",
                keyValue: new Guid("018fdb67-20c0-7c09-8aa3-818bc80648f6"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodMitigations",
                keyColumn: "Id",
                keyValue: new Guid("018fdb68-0b20-7840-bbb4-4cc1120720ac"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodMitigations",
                keyColumn: "Id",
                keyValue: new Guid("018fdb68-f580-761f-8f45-805d18c65823"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodMitigations",
                keyColumn: "Id",
                keyValue: new Guid("018fdb69-dfe0-735a-ba5a-389eb2f5f753"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodMitigations",
                keyColumn: "Id",
                keyValue: new Guid("018fdb6a-ca40-7e06-b6cd-0dab09a39e90"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodMitigations",
                keyColumn: "Id",
                keyValue: new Guid("018fdb6b-b4a0-77c5-9721-e3a1cac011fa"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodMitigations",
                keyColumn: "Id",
                keyValue: new Guid("018fdb6c-9f00-732f-9d67-a087fa117a8a"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodMitigations",
                keyColumn: "Id",
                keyValue: new Guid("018fdb6d-8960-7e00-a446-056d1f74e329"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodMitigations",
                keyColumn: "Id",
                keyValue: new Guid("018fdb6e-73c0-71c8-a533-0bff3d55eb59"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodMitigations",
                keyColumn: "Id",
                keyValue: new Guid("018fdb9c-3a80-7300-8a49-5b3df75adf2a"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodMitigations",
                keyColumn: "Id",
                keyValue: new Guid("018fdb9d-24e0-78ba-9fff-95ac94b38f7c"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodMitigations",
                keyColumn: "Id",
                keyValue: new Guid("018fdb9e-0f40-7061-959a-23bfb2bba985"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodMitigations",
                keyColumn: "Id",
                keyValue: new Guid("018fdb9e-f9a0-70ba-9475-5793cbf66ece"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodMitigations",
                keyColumn: "Id",
                keyValue: new Guid("018fdb9f-e400-77bf-868a-633a7e27bc8c"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodMitigations",
                keyColumn: "Id",
                keyValue: new Guid("018fdba0-ce60-77e4-a0a6-91ab51596fad"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodMitigations",
                keyColumn: "Id",
                keyValue: new Guid("018fdba1-b8c0-765c-89f6-bd1d9019db0c"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodMitigations",
                keyColumn: "Id",
                keyValue: new Guid("018fdba2-a320-793d-afd9-126986a9a3fb"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodMitigations",
                keyColumn: "Id",
                keyValue: new Guid("018fdbd3-2900-7fed-9c52-9f4668e28618"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodMitigations",
                keyColumn: "Id",
                keyValue: new Guid("018fdbd4-1360-7df8-80a2-a8ae26685016"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodMitigations",
                keyColumn: "Id",
                keyValue: new Guid("018fdbd4-fdc0-71a4-8973-f5efce80c875"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodMitigations",
                keyColumn: "Id",
                keyValue: new Guid("018fdbd5-e820-7eb6-bee7-d1de8738d312"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodMitigations",
                keyColumn: "Id",
                keyValue: new Guid("018fdbd6-d280-7d17-9fc4-78608036bd36"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodMitigations",
                keyColumn: "Id",
                keyValue: new Guid("018fdbd7-bce0-7545-8aa2-a19a5bda2e83"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodMitigations",
                keyColumn: "Id",
                keyValue: new Guid("018fdbd8-a740-7dbf-9f3b-393e01305c25"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodMitigations",
                keyColumn: "Id",
                keyValue: new Guid("018fdbd9-91a0-763d-9773-de4670ae0781"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodMitigations",
                keyColumn: "Id",
                keyValue: new Guid("018fdc0a-1780-793d-9239-fe2a17b52571"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodMitigations",
                keyColumn: "Id",
                keyValue: new Guid("018fdc0b-01e0-78f7-864b-0297b744acad"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodMitigations",
                keyColumn: "Id",
                keyValue: new Guid("018fdc0b-ec40-7769-be33-afc5bf808f01"),
                column: "TypeDescription",
                value: null);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodMitigations",
                keyColumn: "Id",
                keyValue: new Guid("018fdc0c-d6a0-73ae-b83c-d13ccfc0da71"),
                column: "TypeDescription",
                value: null);
        }
    }
}

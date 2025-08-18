using FloodOnlineReportingTool.Database.Models;
using FloodOnlineReportingTool.Database.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FloodOnlineReportingTool.Database.EntitiesConfiguration;

internal class FloodResponsibilityConfiguration : IEntityTypeConfiguration<FloodResponsibility>
{
    public void Configure(EntityTypeBuilder<FloodResponsibility> builder)
    {
        builder
            .HasKey(o => new { o.OrganisationId, o.AdminUnitId });

        builder
            .Property(o => o.OrganisationId)
            .ValueGeneratedNever();

        builder
            .Property(o => o.AdminUnitId)
            .ValueGeneratedNever();

        builder
            .ToTable(o => o.HasComment("Areas responsible for handling flood reports/eligibility checks."));

        builder
            .HasData(InitialData.FloodResponsibilityData());

        // Auto includes
        builder
            .Navigation(o => o.Organisation)
            .AutoInclude();
    }
}

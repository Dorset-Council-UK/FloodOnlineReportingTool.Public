using FloodOnlineReportingTool.Database.Models.Investigate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FloodOnlineReportingTool.Database.EntitiesConfiguration;

internal class InvestigationServiceImpactConfiguration : IEntityTypeConfiguration<InvestigationServiceImpact>
{
    public void Configure(EntityTypeBuilder<InvestigationServiceImpact> builder)
    {
        builder
            .HasKey(o => new { o.InvestigationId, o.FloodImpactId });

        builder
            .Property(o => o.InvestigationId)
            .ValueGeneratedNever();

        builder
            .Property(o => o.FloodImpactId)
            .ValueGeneratedNever();

        builder
            .ToTable(o => o.HasComment("Relationships between investigation and service flood impacts"));

        // Auto includes
        builder
            .Navigation(o => o.FloodImpact)
            .AutoInclude();
    }
}

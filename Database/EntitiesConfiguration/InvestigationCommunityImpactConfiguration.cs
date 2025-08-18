using FloodOnlineReportingTool.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FloodOnlineReportingTool.Database.EntitiesConfiguration;

internal class InvestigationCommunityImpactConfiguration : IEntityTypeConfiguration<InvestigationCommunityImpact>
{
    public void Configure(EntityTypeBuilder<InvestigationCommunityImpact> builder)
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
            .ToTable(o => o.HasComment("Relationships between investigation and community flood impacts"));

        // Auto includes
        builder
            .Navigation(o => o.FloodImpact)
            .AutoInclude();
    }
}

using FloodOnlineReportingTool.Database.Models.Eligibility;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FloodOnlineReportingTool.Database.EntitiesConfiguration;

internal class EligibilityCheckResidentialConfiguration : IEntityTypeConfiguration<EligibilityCheckResidential>
{
    public void Configure(EntityTypeBuilder<EligibilityCheckResidential> builder)
    {
        builder
            .HasKey(o => new { o.EligibilityCheckId, o.FloodImpactId });

        builder
            .Property(o => o.EligibilityCheckId)
            .ValueGeneratedNever();

        builder
            .Property(o => o.FloodImpactId)
            .ValueGeneratedNever();

        builder
            .ToTable(o => o.HasComment("Relationships between eligibility checks and residential flood impacts"));

        // Auto includes
        builder
            .Navigation(o => o.FloodImpact)
            .AutoInclude();
    }
}

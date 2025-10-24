using FloodOnlineReportingTool.Database.Models.Eligibility;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FloodOnlineReportingTool.Database.EntitiesConfiguration;

internal class EligibilityCheckRunoffSourceConfiguration : IEntityTypeConfiguration<EligibilityCheckRunoffSource>
{
    public void Configure(EntityTypeBuilder<EligibilityCheckRunoffSource> builder)
    {
        builder
            .HasKey(o => new { o.EligibilityCheckId, o.FloodProblemId });

        builder
            .Property(o => o.EligibilityCheckId)
            .ValueGeneratedNever();

        builder
            .Property(o => o.FloodProblemId)
            .ValueGeneratedNever();

        builder
            .ToTable(o => o.HasComment("Relationships between eligibility checks and source runoff flood problems"));

        // Auto includes
        builder
            .Navigation(o => o.FloodProblem)
            .AutoInclude();
    }
}

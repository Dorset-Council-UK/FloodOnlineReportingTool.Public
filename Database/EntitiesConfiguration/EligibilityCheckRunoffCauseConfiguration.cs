using FloodOnlineReportingTool.Database.Models.Eligibility;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FloodOnlineReportingTool.Database.EntitiesConfiguration;

internal class EligibilityCheckRunoffCauseConfiguration : IEntityTypeConfiguration<EligibilityCheckRunoffCause>
{
    public void Configure(EntityTypeBuilder<EligibilityCheckRunoffCause> builder)
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
            .ToTable(o => o.HasComment("Relationships between eligibility checks and cause runoff flood problems"));

        // Auto includes
        builder
            .Navigation(o => o.FloodProblem)
            .AutoInclude();
    }
}

using FloodOnlineReportingTool.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FloodOnlineReportingTool.Database.EntitiesConfiguration;

internal class EligibilityCheckConfiguration : IEntityTypeConfiguration<EligibilityCheck>
{
    public void Configure(EntityTypeBuilder<EligibilityCheck> builder)
    {
        builder
            .Property(o => o.Id)
            .ValueGeneratedNever();

        builder
            .ToTable(o => o.HasComment("Eligibility assessments to determine if a person qualifies for assistance, related to flood damage"));

        builder
            .Property(o => o.VulnerablePeopleId)
            .HasDefaultValue(RecordStatusIds.NotSure);

        // Auto includes
        builder
            .Navigation(o => o.Residentials)
            .AutoInclude();

        builder
            .Navigation(o => o.Commercials)
            .AutoInclude();

        builder
            .Navigation(o => o.Sources)
            .AutoInclude();

        builder
            .Navigation(o => o.VulnerablePeople)
            .AutoInclude();
    }
}

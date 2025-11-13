using FloodOnlineReportingTool.Database.Models.Investigate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FloodOnlineReportingTool.Database.EntitiesConfiguration;

internal class InvestigationEntryConfiguration : IEntityTypeConfiguration<InvestigationEntry>
{
    public void Configure(EntityTypeBuilder<InvestigationEntry> builder)
    {
        builder
            .HasKey(o => new { o.InvestigationId, o.FloodProblemId });

        builder
            .Property(o => o.InvestigationId)
            .ValueGeneratedNever();

        builder
            .Property(o => o.FloodProblemId)
            .ValueGeneratedNever();

        builder
            .ToTable(o => o.HasComment("Relationships between investigation and entry flood problems"));

        // Auto includes
        builder
            .Navigation(o => o.FloodProblem)
            .AutoInclude();
    }
}

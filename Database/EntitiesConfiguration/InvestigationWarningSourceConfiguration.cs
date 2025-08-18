using FloodOnlineReportingTool.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FloodOnlineReportingTool.Database.EntitiesConfiguration;

internal class InvestigationWarningSourceConfiguration : IEntityTypeConfiguration<InvestigationWarningSource>
{
    public void Configure(EntityTypeBuilder<InvestigationWarningSource> builder)
    {
        builder
            .HasKey(o => new { o.InvestigationId, o.FloodMitigationId });

        builder
            .Property(o => o.InvestigationId)
            .ValueGeneratedNever();

        builder
            .Property(o => o.FloodMitigationId)
            .ValueGeneratedNever();

        builder
            .ToTable(o => o.HasComment("Relationships between investigation and water source flood mitigations"));

        // Auto includes
        builder
            .Navigation(o => o.FloodMitigation)
            .AutoInclude();
    }
}

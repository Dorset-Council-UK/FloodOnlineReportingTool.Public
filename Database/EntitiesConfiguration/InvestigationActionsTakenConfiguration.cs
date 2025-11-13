using FloodOnlineReportingTool.Database.Models.Investigate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FloodOnlineReportingTool.Database.EntitiesConfiguration;

internal class InvestigationActionsTakenConfiguration : IEntityTypeConfiguration<InvestigationActionsTaken>
{
    public void Configure(EntityTypeBuilder<InvestigationActionsTaken> builder)
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
            .ToTable(o => o.HasComment("Relationships between investigation and actions taken for the flood"));

        // Auto includes
        builder
            .Navigation(o => o.FloodMitigation)
            .AutoInclude();
    }
}

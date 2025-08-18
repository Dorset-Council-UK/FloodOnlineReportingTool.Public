using FloodOnlineReportingTool.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FloodOnlineReportingTool.Database.EntitiesConfiguration;

internal class InvestigationHelpReceivedConfiguration : IEntityTypeConfiguration<InvestigationHelpReceived>
{
    public void Configure(EntityTypeBuilder<InvestigationHelpReceived> builder)
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
            .ToTable(o => o.HasComment("Relationships between investigation and help received for the flood"));

        // Auto includes
        builder
            .Navigation(o => o.FloodMitigation)
            .AutoInclude();
    }
}

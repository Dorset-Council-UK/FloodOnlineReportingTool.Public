using FloodOnlineReportingTool.Database.Models.Investigate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FloodOnlineReportingTool.Database.EntitiesConfiguration;

internal class InvestigationPeakDepthConfiguration : IEntityTypeConfiguration<InvestigationPeakDepth>
{
    public void Configure(EntityTypeBuilder<InvestigationPeakDepth> builder)
    {
        builder
            .Property(o => o.Id)
            .ValueGeneratedNever();

        builder
            .ToTable(o => o.HasComment("Investigations peak depths"));

        builder
            .Property(o => o.DepthLocation)
            .HasMaxLength(200);
    }
}

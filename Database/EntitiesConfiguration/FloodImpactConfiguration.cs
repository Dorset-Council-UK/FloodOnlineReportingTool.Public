using FloodOnlineReportingTool.Database.Models.Flood;
using FloodOnlineReportingTool.Database.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FloodOnlineReportingTool.Database.EntitiesConfiguration;

internal class FloodImpactConfiguration : IEntityTypeConfiguration<FloodImpact>
{
    public void Configure(EntityTypeBuilder<FloodImpact> builder)
    {
        builder
            .Property(o => o.Id)
            .ValueGeneratedNever();

        builder
            .ToTable(o => o.HasComment("Represents the broader impacts of a flood, such as health risks, economic losses, etc"));

        builder
            .HasData(InitialData.FloodImpactData());

        builder
            .HasIndex(o => o.Category);
    }
}

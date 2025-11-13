using FloodOnlineReportingTool.Database.Models.Responsibilities;
using FloodOnlineReportingTool.Database.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FloodOnlineReportingTool.Database.EntitiesConfiguration;

internal class FloodAuthorityConfiguration : IEntityTypeConfiguration<FloodAuthority>
{
    public void Configure(EntityTypeBuilder<FloodAuthority> builder)
    {
        builder
            .Property(o => o.Id)
            .ValueGeneratedNever();

        builder
            .ToTable(o => o.HasComment("Authorities and agencies responsible for managing, mitigating, and responding to flood risks and incidents"));

        builder
            .HasData(InitialData.FloodAuthorityData());
    }
}

using FloodOnlineReportingTool.Database.Models;
using FloodOnlineReportingTool.Database.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FloodOnlineReportingTool.Database.EntitiesConfiguration;

internal class OrganisationConfiguration : IEntityTypeConfiguration<Organisation>
{
    public void Configure(EntityTypeBuilder<Organisation> builder)
    {
        builder
            .Property(o => o.Id)
            .ValueGeneratedNever();

        builder
            .ToTable(o => o.HasComment("Represents specific geographic administrative areas, responsible for flood management"));

        builder
            .HasData(InitialData.OrganisationData());

        // Auto includes
        builder
            .Navigation(o => o.FloodAuthority)
            .AutoInclude();
    }
}

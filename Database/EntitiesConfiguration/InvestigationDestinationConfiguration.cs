using FloodOnlineReportingTool.Database.Models.Investigate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FloodOnlineReportingTool.Database.EntitiesConfiguration;

internal class InvestigationDestinationConfiguration : IEntityTypeConfiguration<InvestigationDestination>
{
    public void Configure(EntityTypeBuilder<InvestigationDestination> builder)
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
            .ToTable(o => o.HasComment("Relationships between investigation and destination flood problems"));

        // Auto includes
        builder
            .Navigation(o => o.FloodProblem)
            .AutoInclude();
    }
}

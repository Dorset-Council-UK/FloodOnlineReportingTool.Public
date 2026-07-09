using FloodOnlineReportingTool.Contracts.Shared;
using FloodOnlineReportingTool.Database.Models.Flood;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FloodOnlineReportingTool.Database.EntitiesConfiguration;

internal class FloodReportSourceConfiguration : IEntityTypeConfiguration<FloodReportSource>
{
    public void Configure(EntityTypeBuilder<FloodReportSource> builder)
    {
        builder
            .ToTable(o => o.HasComment("Flood report sources"));

        builder.HasKey(o => o.Id);
        // Unique constraint ensures Reference is consistent across FORT modules
        builder.HasAlternateKey(o => o.Reference);

        builder
            .Property(o => o.Id)
            .ValueGeneratedNever();

        builder
            .Property(o => o.Reference)
            .HasMaxLength(15);

        builder
            .Property(o => o.StatusId)
            .HasDefaultValue(RecordStatusIds.New);

        // Soft deletion filter
        builder
            .HasQueryFilter(o => o.StatusId != RecordStatusIds.MarkedForDeletion);
    }
}

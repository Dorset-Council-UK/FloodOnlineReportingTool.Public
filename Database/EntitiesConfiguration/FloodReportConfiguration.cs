using FloodOnlineReportingTool.Contracts.Shared;
using FloodOnlineReportingTool.Database.Models.Flood;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FloodOnlineReportingTool.Database.EntitiesConfiguration;

internal class FloodReportConfiguration : IEntityTypeConfiguration<FloodReport>
{
    public void Configure(EntityTypeBuilder<FloodReport> builder)
    {
        builder
            .ToTable(o => o.HasComment("Flood report overviews"));

        builder
            .Property(o => o.Id)
            .ValueGeneratedNever();

        builder
            .HasIndex(o => o.Id)
            .IsUnique();

        builder
            .HasIndex(o => o.Reference)
            .IsUnique();

        builder
            .Property(o => o.Reference)
            .HasMaxLength(8);

        builder
            .Property(o => o.StatusId)
            .HasDefaultValue(RecordStatusIds.New);

        // Explicitly configure the ReportOwner relationship
        //builder
        //    .HasOne(o => o.ReportOwner)
        //    .WithMany()
        //    .HasForeignKey(o => o.ReportOwnerId)
        //    .IsRequired(false)
        //    .OnDelete(DeleteBehavior.SetNull);

        // Soft deletion filter
        builder
            .HasQueryFilter(o => o.StatusId != RecordStatusIds.MarkedForDeletion);
    }
}

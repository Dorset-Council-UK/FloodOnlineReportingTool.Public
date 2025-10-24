using FloodOnlineReportingTool.Contracts.Shared;
using FloodOnlineReportingTool.Database.Models.Contact;
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

        // Owner: many FloodReports -> one ContactRecord
        // NOTE: do NOT make ReportOwnerId unique if a ContactRecord can own multiple reports
        builder
            .HasOne(fr => fr.ReportOwner)
            .WithMany(cr => cr.OwnedFloodReports)
            .HasForeignKey(fr => fr.ReportOwnerId)
            .OnDelete(DeleteBehavior.SetNull);

        // ExtraContactRecords: explicit many-to-many
        builder
            .HasMany(fr => fr.ExtraContactRecords)
            .WithMany(cr => cr.FloodReports)
            .UsingEntity<Dictionary<string, object>>(
                "FloodReportContactRecord",
                j => j
                    .HasOne<ContactRecord>()
                    .WithMany()
                    .HasForeignKey("ContactRecordId")
                    .HasConstraintName("FK_FloodReportContactRecord_ContactRecords_ContactRecordId")
                    .OnDelete(DeleteBehavior.Cascade),
                j => j
                    .HasOne<FloodReport>()
                    .WithMany()
                    .HasForeignKey("FloodReportId")
                    .HasConstraintName("FK_FloodReportContactRecord_FloodReports_FloodReportId")
                    .OnDelete(DeleteBehavior.Cascade),
                j =>
                {
                    j.ToTable("FloodReportContactRecords", "fortpublic");
                    j.HasKey("FloodReportId", "ContactRecordId");
                });

        // Map SingleAssociatedContacts (reverse of ContactRecord.FloodReport)
        builder
            .HasMany(fr => fr.SingleAssociatedContacts)
            .WithOne(cr => cr.FloodReport)
            .HasForeignKey(cr => cr.FloodReportId)
            .OnDelete(DeleteBehavior.Cascade);

        // Soft deletion filter
        builder
            .HasQueryFilter(o => o.StatusId != RecordStatusIds.MarkedForDeletion);
    }
}

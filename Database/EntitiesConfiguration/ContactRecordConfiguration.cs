using FloodOnlineReportingTool.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FloodOnlineReportingTool.Database.EntitiesConfiguration;

internal class ContactRecordConfiguration : IEntityTypeConfiguration<ContactRecord>
{
    public void Configure(EntityTypeBuilder<ContactRecord> builder)
    {
        builder
            .Property(o => o.Id)
            .ValueGeneratedNever();

        // Partial unique index: when ContactUserId IS NULL (non-user contact),
        // FloodReportId must be unique → enforces at most one FloodReport association for such contacts.
        // This creates a PostgreSQL partial index.
        builder
            .HasIndex(cr => cr.FloodReportId)
            .HasDatabaseName("IX_ContactRecords_FloodReportId_UniqueWhenNoUser")
            .IsUnique()
            .HasFilter("\"ContactUserId\" IS NULL");

        builder
            .ToTable(o => o.HasComment("Contact information for individuals reporting flood incidents and seeking assistance"));
    }
}

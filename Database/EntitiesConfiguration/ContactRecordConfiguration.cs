using FloodOnlineReportingTool.Database.Models.Contact;
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

        builder
            .HasIndex(o => o.ContactUserId);

        // Many-to-many: ContactRecord <-> FloodReport
        builder
            .HasMany(cr => cr.FloodReports)
            .WithMany(fr => fr.ContactRecords);

        builder
            .ToTable(o => o.HasComment("Contact information for individuals reporting flood incidents and seeking assistance"));
    }
}

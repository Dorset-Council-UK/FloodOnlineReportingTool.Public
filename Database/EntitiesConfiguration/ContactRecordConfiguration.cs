using FloodOnlineReportingTool.Database.Models.Contact;
using FloodOnlineReportingTool.Database.Models.Contact.Subscribe;
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

        // One-to-one: ContactRecord -> SubscribeRecord
        // ContactRecord is principal, SubscribeRecord is dependent
        // ContactRecord.SubscribeRecordId is the foreign key
        builder
            .HasOne(cr => cr.SubscribeRecord)
            .WithOne(sr => sr.ContactRecord)
            .HasForeignKey<SubscribeRecord>(sr => sr.ContactRecordId)
            .IsRequired(false);

        // Many-to-many: ContactRecord <-> FloodReport
        builder
            .HasMany(cr => cr.FloodReports)
            .WithMany(fr => fr.ContactRecords);

        builder
            .ToTable(o => o.HasComment("Contact information for individuals reporting flood incidents and seeking assistance"));
    }
}

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

        builder
            .HasMany(fr => fr.FloodReports)
            .WithOne()
            .OnDelete(DeleteBehavior.NoAction); // Optional: define delete behavior

        builder
            .ToTable(o => o.HasComment("Contact information for individuals reporting flood incidents and seeking assistance"));
    }
}

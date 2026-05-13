using FloodOnlineReportingTool.Database.Models.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FloodOnlineReportingTool.Database.EntitiesConfiguration;

internal class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder
            .Property(o => o.Id)
            .ValueGeneratedNever();

        // Supports the outbox worker polling query:
        // WHERE Status = Pending ORDER BY Created TAKE 10
        builder
            .HasIndex(o => new { o.Status, o.Created });

        builder
            .ToTable(o => o.HasComment("The outbox message table is used to store messages that need to be sent to other systems. It is part of a messaging outbox pattern."));
    }
}

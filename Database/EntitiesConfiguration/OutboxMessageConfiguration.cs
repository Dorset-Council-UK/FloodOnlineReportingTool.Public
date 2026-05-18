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

        builder
            .HasIndex(o => new { o.Priority, o.Created });

        builder
            .HasIndex(o => new { o.Status, o.Priority, o.Created });

        builder
            .ToTable(o => o.HasComment("The outbox message table is used to store messages that need to be sent to other systems. It is part of a messaging outbox pattern."));
    }
}

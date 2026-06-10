using FloodOnlineReportingTool.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FloodOnlineReportingTool.Database.EntitiesConfiguration
{
    internal class MediaItemConfiguration : IEntityTypeConfiguration<MediaItem>
    {
        public void Configure(EntityTypeBuilder<MediaItem> builder)
        {
            builder
                .Property(o => o.Id)
                .ValueGeneratedNever();

            builder
                .ToTable(o => o.HasComment("Media items linked to flood reports"));

        }
    }
}

using FloodOnlineReportingTool.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace FloodOnlineReportingTool.Database.DbContexts;

public class BoundariesDbContext(DbContextOptions<BoundariesDbContext> options) : DbContext(options)
{
    public DbSet<UkCounty> Counties { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Make this context read-only
        optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresExtension("postgis");
        modelBuilder.HasDefaultSchema(SchemaNames.Boundaries);

        // Entity configuration
        modelBuilder
            .Entity<UkCounty>()
            .ToTable("uk_county", t => t.ExcludeFromMigrations())
            .HasKey(o => o.Name);

        modelBuilder
            .Entity<UkCounty>()
            .Property(o => o.Name)
            .HasColumnName(name: "name");

        //modelBuilder
        //    .Entity<UkCounty>()
        //    .Property(o => o.AreaCode)
        //    .HasColumnName(name: "area_code");

        modelBuilder
            .Entity<UkCounty>()
            .Property(o => o.AreaDescription)
            .HasColumnName(name: "area_description");

        modelBuilder
            .Entity<UkCounty>()
            .Property(o => o.AdminUnitId)
            .HasColumnName(name: "admin_unit_id");

        //modelBuilder
        //    .Entity<UkCounty>()
        //    .Property(o => o.Location)
        //    .HasColumnName(name: "geom");
    }
}
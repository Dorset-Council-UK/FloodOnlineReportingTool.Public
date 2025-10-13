using FloodOnlineReportingTool.Database.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace FloodOnlineReportingTool.Database.DbContexts;

public class PublicDbContext(DbContextOptions<PublicDbContext> options) : DbContext(options)
{
    public DbSet<ContactRecord> ContactRecords { get; set; }
    public DbSet<EligibilityCheck> EligibilityChecks { get; set; }
    public DbSet<EligibilityCheckResidential> EligibilityCheckResidentials { get; set; } // Relationship table
    public DbSet<EligibilityCheckCommercial> EligibilityCheckCommercials { get; set; } // Relationship table
    public DbSet<EligibilityCheckSource> EligibilityCheckSources { get; set; } // Relationship table
    public DbSet<FloodAuthority> FloodAuthorities { get; set; }
    public DbSet<FloodAuthorityFloodProblem> FloodAuthorityFloodProblems { get; set; } // Relationship table
    public DbSet<FloodImpact> FloodImpacts { get; set; }
    public DbSet<FloodMitigation> FloodMitigations { get; set; }
    public DbSet<FloodProblem> FloodProblems { get; set; }
    public DbSet<FloodReport> FloodReports { get; set; }
    public DbSet<FloodReportContact> FloodReportContacts { get; set; } // Relationship table
    public DbSet<FloodResponsibility> FloodResponsibilities { get; set; }
    public DbSet<Investigation> Investigations { get; set; }
    public DbSet<InvestigationDestination> InvestigationDestinations { get; set; } // Relationship table
    public DbSet<InvestigationEntry> InvestigationEntries { get; set; } // Relationship table
    public DbSet<InvestigationWarningSource> InvestigationWarningSources { get; set; } // Relationship table
    public DbSet<Organisation> Organisations { get; set; }
    public DbSet<RecordStatus> RecordStatuses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(SchemaNames.FortPublic);

        // Define a many-to-many relationship
        modelBuilder.Entity<FloodReportContact>()
                .HasOne(fc => fc.FloodReport)
                .WithMany(fr => fr.ContactRecords)
                .HasForeignKey(fc => fc.FloodReportId);

        modelBuilder.Entity<FloodReportContact>()
            .HasOne(fc => fc.ContactRecord)
            .WithMany(cr => cr.FloodReports)
            .HasForeignKey(fc => fc.ContactRecordId);

        // Enforce one ContactType per FloodReport
        modelBuilder.Entity<FloodReportContact>()
            .HasIndex(fc => new { fc.FloodReportId, fc.ContactType })
            .IsUnique();

        // Add the inbox and outbox pattern messaging tables
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();

        // Entity configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PublicDbContext).Assembly);
    }
}
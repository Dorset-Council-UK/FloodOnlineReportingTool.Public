using FloodOnlineReportingTool.Database.Models.Contact;
using FloodOnlineReportingTool.Database.Models.Contact.Subscribe;
using FloodOnlineReportingTool.Database.Models.Eligibility;
using FloodOnlineReportingTool.Database.Models.Flood;
using FloodOnlineReportingTool.Database.Models.Investigate;
using FloodOnlineReportingTool.Database.Models.Messaging;
using FloodOnlineReportingTool.Database.Models.Responsibilities;
using FloodOnlineReportingTool.Database.Models.Status;
using Microsoft.EntityFrameworkCore;

namespace FloodOnlineReportingTool.Database.DbContexts;

public class PublicDbContext(DbContextOptions<PublicDbContext> options) : DbContext(options)
{
    public DbSet<ContactRecord> ContactRecords { get; set; }
    public DbSet<SubscribeRecord> ContactSubscribeRecords { get; set; }
    public DbSet<EligibilityCheck> EligibilityChecks { get; set; }
    public DbSet<EligibilityCheckResidential> EligibilityCheckResidentials { get; set; } // Relationship table
    public DbSet<EligibilityCheckCommercial> EligibilityCheckCommercials { get; set; } // Relationship table
    public DbSet<EligibilityCheckCause> EligibilityCheckCauses { get; set; } // Relationship table
    public DbSet<FloodAuthority> FloodAuthorities { get; set; }
    public DbSet<FloodAuthorityFloodProblem> FloodAuthorityFloodProblems { get; set; } // Relationship table
    public DbSet<FloodImpact> FloodImpacts { get; set; }
    public DbSet<FloodMitigation> FloodMitigations { get; set; }
    public DbSet<FloodProblem> FloodProblems { get; set; }
    public DbSet<FloodReportSource> FloodReportSources { get; set; }
    public DbSet<FloodResponsibility> FloodResponsibilities { get; set; }
    public DbSet<Investigation> Investigations { get; set; }
    public DbSet<InvestigationDestination> InvestigationDestinations { get; set; } // Relationship table
    public DbSet<InvestigationEntry> InvestigationEntries { get; set; } // Relationship table
    public DbSet<InvestigationWarningSource> InvestigationWarningSources { get; set; } // Relationship table
    public DbSet<Organisation> Organisations { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
    public DbSet<RecordStatus> RecordStatuses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(SchemaNames.FortPublic);

        // Entity configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PublicDbContext).Assembly);
    }
}
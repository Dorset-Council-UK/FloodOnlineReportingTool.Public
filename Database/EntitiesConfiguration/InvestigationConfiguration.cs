using FloodOnlineReportingTool.Database.Models.Investigate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FloodOnlineReportingTool.Database.EntitiesConfiguration;

#pragma warning disable MA0051 // Method is too long

internal class InvestigationConfiguration : IEntityTypeConfiguration<Investigation>
{
    public void Configure(EntityTypeBuilder<Investigation> builder)
    {
        builder
            .Property(o => o.Id)
            .ValueGeneratedNever();

        builder
            .ToTable(o => o.HasComment("Investigations, for example grants"));

        builder
            .Property(o => o.MoreAppearanceDetails)
            .HasMaxLength(200);

        builder
            .Property(o => o.WaterEnteredOther)
            .HasMaxLength(100);

        builder
            .Property(o => o.HistoryOfFloodingDetails)
            .HasMaxLength(200);

        builder
            .Property(o => o.WarningSourceOther)
            .HasMaxLength(100);

        builder
            .Property(o => o.KnownProblemDetails)
            .HasMaxLength(200);

        builder
            .Property(o => o.OtherAction)
            .HasMaxLength(100);

        SetupAutoIncludes(builder);
    }

    /// <summary>
    /// Set up auto includes for the investigation table.
    /// </summary>
    private static void SetupAutoIncludes(EntityTypeBuilder<Investigation> builder)
    {
        builder
            .Navigation(o => o.HistoryOfFlooding)
            .AutoInclude();

        builder
            .Navigation(o => o.Begin)
            .AutoInclude();

        builder
            .Navigation(o => o.WaterSpeed)
            .AutoInclude();

        builder
            .Navigation(o => o.Appearance)
            .AutoInclude();

        builder
            .Navigation(o => o.Entries)
            .AutoInclude();

        builder
            .Navigation(o => o.Destinations)
            .AutoInclude();

        builder
            .Navigation(o => o.WereVehiclesDamaged)
            .AutoInclude();

        builder
            .Navigation(o => o.ServiceImpacts)
            .AutoInclude();

        builder
            .Navigation(o => o.CommunityImpacts)
            .AutoInclude();

        builder
            .Navigation(o => o.HelpReceived)
            .AutoInclude();

        builder
            .Navigation(o => o.ActionsTaken)
            .AutoInclude();

        builder
            .Navigation(o => o.Floodline)
            .AutoInclude();

        builder
            .Navigation(o => o.WarningReceived)
            .AutoInclude();

        builder
            .Navigation(o => o.WarningTimely)
            .AutoInclude();

        builder
            .Navigation(o => o.WarningAppropriate)
            .AutoInclude();

        builder
            .Navigation(o => o.PropertyInsured)
            .AutoInclude();
    }
}

namespace FloodOnlineReportingTool.Database.Models;

public static class InvestigationsDtoExtensions
{
    /// <summary>
    ///     <para>Converts an investigation DTO to an investigation entity.</para>
    ///     <para>The new investigation ID must be passsed in to this method.</para>
    /// </summary>
    public static Investigation ToInvestigation(this InvestigationDto dto, Guid investigationId, bool isInternal)
    {
        var investigation = CreateBaseInvestigation(dto, investigationId);
        investigation = ApplyInternalFields(dto, investigationId, isInternal, investigation);
        investigation = ApplyPeakDepth(dto, investigation);
        investigation = ApplyFloodlineWarnings(dto, investigation);

        return investigation;
    }

    /// <summary>
    /// Applies floodline warnings to the investigation only if the floodline warning source is present.
    /// </summary>
    private static Investigation ApplyFloodlineWarnings(InvestigationDto dto, Investigation investigation)
    {
        if (!dto.WarningSources.Contains(FloodMitigationIds.FloodlineWarning))
        {
            return investigation;
        }

        return investigation with
        {
            WarningTimelyId = dto.WarningTimelyId,
            WarningAppropriateId = dto.WarningAppropriateId,
        };
    }

    /// <summary>
    /// Applies peak depth fields to the investigation only if it is known.
    /// </summary>
    private static Investigation ApplyPeakDepth(InvestigationDto dto, Investigation investigation)
    {
        if (dto.IsPeakDepthKnownId != RecordStatusIds.Yes)
        {
            return investigation;
        }

        return investigation with
        {
            PeakInsideCentimetres = dto.PeakInsideCentimetres!.Value,
            PeakOutsideCentimetres = dto.PeakOutsideCentimetres!.Value,
        };
    }

    /// <summary>
    /// Applies internal fields to the investigation only if it is internal.
    /// </summary>
    private static Investigation ApplyInternalFields(InvestigationDto dto, Guid investigationId, bool isInternal, Investigation investigation)
    {
        if (!isInternal)
        {
            return investigation;
        }

        return investigation with
        {
            // Internal - How it entered - Water entry
            Entries = [.. dto.Entries.Select(floodProblemId => new InvestigationEntry(investigationId, floodProblemId))],
            WaterEnteredOther = dto.WaterEnteredOther,

            // Internal - When it entered
            WhenWaterEnteredKnownId = dto.WhenWaterEnteredKnownId!.Value,
            FloodInternalUtc = dto.FloodInternalUtc,
        };
    }

    /// <summary>
    /// Creates the base investigation entity from the DTO.
    /// </summary>
    private static Investigation CreateBaseInvestigation(InvestigationDto dto, Guid investigationId)
    {
        return new Investigation
        {
            Id = investigationId,

            // Water speed
            BeginId = dto.BeginId!.Value,
            WaterSpeedId = dto.WaterSpeedId!.Value,
            AppearanceId = dto.AppearanceId!.Value,
            MoreAppearanceDetails = dto.MoreAppearanceDetails,

            // Water destination
            Destinations = [.. dto.Destinations.Select(floodProblemId => new InvestigationDestination(investigationId, floodProblemId))],

            // Damaged vehicles
            WereVehiclesDamagedId = dto.WereVehiclesDamagedId!.Value,
            NumberOfVehiclesDamaged = dto.NumberOfVehiclesDamaged,

            // Internal (handled below)

            // Peak depth (handled below)
            IsPeakDepthKnownId = dto.IsPeakDepthKnownId!.Value,

            // Community impact
            CommunityImpacts = [.. dto.CommunityImpacts.Select(floodImpactId => new InvestigationCommunityImpact(investigationId, floodImpactId))],

            // Blockages
            HasKnownProblems = dto.HasKnownProblems == true,
            KnownProblemDetails = dto.KnownProblemDetails,

            // Actions taken
            ActionsTaken = [.. dto.ActionsTaken.Select(floodMitigationId => new InvestigationActionsTaken(investigationId, floodMitigationId))],
            OtherAction = dto.OtherAction,

            // Help received
            HelpReceived = [.. dto.HelpReceived.Select(floodMitigationId => new InvestigationHelpReceived(investigationId, floodMitigationId))],

            // Before the flooding - Warnings
            FloodlineId = dto.FloodlineId!.Value,
            WarningReceivedId = dto.WarningReceivedId!.Value,

            // Warning sources
            WarningSources = [.. dto.WarningSources.Select(floodMitigationId => new InvestigationWarningSource(investigationId, floodMitigationId))],
            WarningSourceOther = dto.WarningSourceOther,

            // History
            HistoryOfFloodingId = dto.HistoryOfFloodingId!.Value,
            HistoryOfFloodingDetails = dto.HistoryOfFloodingDetails,
        };
    }
}

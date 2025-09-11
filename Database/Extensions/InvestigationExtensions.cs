using FloodOnlineReportingTool.Contracts;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace FloodOnlineReportingTool.Database.Models;
#pragma warning restore IDE0130 // Namespace does not match folder structure

internal static class InvestigationExtensions
{
    internal static InvestigationCreated ToMessageCreated(this Investigation investigation, string floodReportReference)
    {
        var registeredWithFloodline = investigation.FloodlineId == RecordStatusIds.Yes;

        // Received other warnings are when WarningReceivedId is Yes, WarningSources is not NoWarning, and dont count WarningSources of FloodMitigationIds.FloodlineWarning
        var otherWarningSources = investigation.WarningSources
            .Where(o =>
                o.FloodMitigationId != FloodMitigationIds.FloodlineWarning &&
                o.FloodMitigationId != FloodMitigationIds.NoWarning
            )
            .ToList();

        return new InvestigationCreated
        {
            FloodReportReference = floodReportReference,
            Id = investigation.Id,
            CreatedUtc = investigation.CreatedUtc,
            HasEntries = investigation.Entries.Any(o => o.FloodProblemId != FloodProblemIds.FloodEntryIds.NotSure),
            HasHistory = investigation.HistoryOfFloodingId == RecordStatusIds.Yes,
            HasPeakDepth = investigation.IsPeakDepthKnownId == RecordStatusIds.Yes,
            HasInternalFlooding = investigation.WhenWaterEnteredKnownId == RecordStatusIds.Yes,
            HasDestination = investigation.Destinations.Any(o => o.FloodProblemId != FloodProblemIds.FloodDestinationIds.NotSure),
            HasDamagedVehicles = investigation.WereVehiclesDamagedId == RecordStatusIds.Yes,
            HasImpactedTheCommunity = investigation.CommunityImpacts.Any(o => o.FloodImpactId != FloodImpactIds.CommunityImpactNotSure),
            HasBlockages = investigation.HasKnownProblems,
            ActionsWereTaken = investigation.ActionsTaken.Any(o => o.FloodMitigationId != FloodMitigationIds.NoActionTaken),
            HelpWasReceived = investigation.HelpReceived.Any(o => o.FloodMitigationId != FloodMitigationIds.NoHelp),
            RegisteredWithFloodline = registeredWithFloodline,
            ReceivedFloodlineWarningInTime = registeredWithFloodline && investigation.WarningTimelyId == RecordStatusIds.Yes,
            ReceivedOtherWarnings = investigation.WarningReceivedId == RecordStatusIds.Yes && otherWarningSources.Count > 0,
        };
    }

    internal static InvestigationDto ToDto(this Investigation investigation)
    {
        return new InvestigationDto
        {
            // Internal fields
            WhenWaterEnteredKnownId = investigation.WhenWaterEnteredKnownId,
            FloodInternalUtc = investigation.FloodInternalUtc,

            // Peak depth fields
            IsPeakDepthKnownId = investigation.IsPeakDepthKnownId,
            PeakInsideCentimetres = investigation.PeakInsideCentimetres,
            PeakOutsideCentimetres = investigation.PeakOutsideCentimetres,

            // History fields
            HistoryOfFloodingId = investigation.HistoryOfFloodingId,
            HistoryOfFloodingDetails = investigation.HistoryOfFloodingDetails,

            // Water speed fields, which are related flood problems
            BeginId = investigation.BeginId,
            WaterSpeedId = investigation.WaterSpeedId,
            AppearanceId = investigation.AppearanceId,
            MoreAppearanceDetails = investigation.MoreAppearanceDetails,

            // Water entry fields, which are related flood problems
            Entries = [.. investigation.Entries.Select(o => o.FloodProblemId)],
            WaterEnteredOther = investigation.WaterEnteredOther,

            // Water destination fields, which are related flood problems
            Destinations = [.. investigation.Destinations.Select(o => o.FloodProblemId)],

            // Vehicle fields
            WereVehiclesDamagedId = investigation.WereVehiclesDamagedId,
            NumberOfVehiclesDamaged = investigation.NumberOfVehiclesDamaged,

            // Blockages fields
            HasKnownProblems = investigation.HasKnownProblems,
            KnownProblemDetails = investigation.KnownProblemDetails,

            // Community impact fields, which are related flood impacts
            CommunityImpacts = [.. investigation.CommunityImpacts.Select(o => o.FloodImpactId)],

            // Help received fields, which are related flood mitigations
            HelpReceived = [.. investigation.HelpReceived.Select(o => o.FloodMitigationId)],

            // Actions taken fields, which are related flood mitigations
            ActionsTaken = [.. investigation.ActionsTaken.Select(o => o.FloodMitigationId)],
            OtherAction = investigation.OtherAction,

            // Warning fields
            FloodlineId = investigation.FloodlineId,
            WarningReceivedId = investigation.WarningReceivedId,

            // Warning source fields
            WarningSources = [.. investigation.WarningSources.Select(o => o.FloodMitigationId)],
            WarningSourceOther = investigation.WarningSourceOther,

            // Floodline warnings fields
            WarningTimelyId = investigation.WarningTimelyId,
            WarningAppropriateId = investigation.WarningAppropriateId,
        };
    }

    /// <summary>
    /// Applies internal fields to the investigation only if it is internal.
    /// </summary>
    internal static Investigation ApplyInternalFields(this Investigation investigation, InvestigationDto dto, bool isInternal)
    {
        if (!isInternal)
        {
            return investigation;
        }

        return investigation with
        {
            // Internal - How it entered - Water entry
            Entries = [.. dto.Entries.Select(floodProblemId => new InvestigationEntry(investigation.Id, floodProblemId))],
            WaterEnteredOther = dto.WaterEnteredOther,

            // Internal - When it entered
            WhenWaterEnteredKnownId = dto.WhenWaterEnteredKnownId!.Value,
            FloodInternalUtc = dto.FloodInternalUtc,
        };
    }

    /// <summary>
    /// Applies peak depth fields to the investigation only if it is known.
    /// </summary>
    internal static Investigation ApplyPeakDepth(this Investigation investigation, InvestigationDto dto)
    {
        if (dto.IsPeakDepthKnownId == null)
        {
            return investigation;
        }

        if (dto.IsPeakDepthKnownId != RecordStatusIds.Yes)
        {
            return investigation with
            {
                IsPeakDepthKnownId = dto.IsPeakDepthKnownId.Value,
            };
        }

        return investigation with
        {
            IsPeakDepthKnownId = dto.IsPeakDepthKnownId.Value,
            PeakInsideCentimetres = dto.PeakInsideCentimetres!.Value,
            PeakOutsideCentimetres = dto.PeakOutsideCentimetres!.Value,
        };
    }

    /// <summary>
    /// Applies floodline warnings to the investigation only if the floodline warning source is present.
    /// </summary>
    internal static Investigation ApplyFloodlineWarnings(this Investigation investigation, InvestigationDto dto)
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
}

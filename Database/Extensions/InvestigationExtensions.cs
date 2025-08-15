using FloodOnlineReportingTool.Contracts;

namespace FloodOnlineReportingTool.Database.Models;

public static class InvestigationExtensions
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
            HasEntries = investigation.Entries.Any(o => o.FloodProblemId != FloodProblemIds.EntryNotSure),
            HasHistory = investigation.HistoryOfFloodingId == RecordStatusIds.Yes,
            HasPeakDepth = investigation.IsPeakDepthKnownId == RecordStatusIds.Yes,
            HasInternalFlooding = investigation.WhenWaterEnteredKnownId == RecordStatusIds.Yes,
            HasDestination = investigation.Destinations.Any(o => o.FloodProblemId != FloodProblemIds.DestinationNotSure),
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

    public static InvestigationDto ToDto(this Investigation investigation)
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
}

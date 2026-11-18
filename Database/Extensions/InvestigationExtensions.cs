using FloodOnlineReportingTool.Contracts;
using FloodOnlineReportingTool.Database.Models.Flood;
using FloodOnlineReportingTool.Database.Models.Flood.FloodProblemIds;
using FloodOnlineReportingTool.Database.Models.Status;

#pragma warning disable MA0051 // Method is too long

namespace FloodOnlineReportingTool.Database.Models.Investigate;

public static class InvestigationExtensions
{
    extension(Investigation investigation)
    {
        internal InvestigationCreated ToMessageCreated(string floodReportSourceReference)
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
                FloodReportReference = floodReportSourceReference,
                Id = investigation.Id,
                CreatedUtc = investigation.CreatedUtc,
                HasEntries = investigation.Entries.Any(o => o.FloodProblemId != FloodEntryIds.NotSure),
                HasHistory = investigation.HistoryOfFloodingId == RecordStatusIds.Yes,
                HasPropertyInsurance = investigation.PropertyInsuredId == RecordStatusIds.Yes,
                // assuming this is how the change to a list of peak depths will be handled here.
                HasPeakDepth = investigation.PeakDepths.Count > 0,
                HasInternalFlooding = investigation.WhenWaterEnteredKnownId == RecordStatusIds.Yes,
                HasDestination = investigation.Destinations.Any(o => o.FloodProblemId != FloodDestinationIds.NotSure),
                HasDamagedVehicles = investigation.WereVehiclesDamagedId == RecordStatusIds.Yes,
                HasImpactedServices = investigation.ServiceImpacts.Any(o => o.FloodImpactId != FloodImpactIds.ServicesNotAffected && o.FloodImpactId != FloodImpactIds.ServiceImpactNotSure),
                HasImpactedTheCommunity = investigation.CommunityImpacts.Any(o => o.FloodImpactId != FloodImpactIds.CommunityImpactNotSure),
                HasBlockages = investigation.HasKnownProblems,
                ActionsWereTaken = investigation.ActionsTaken.Any(o => o.FloodMitigationId != FloodMitigationIds.NoActionTaken),
                HelpWasReceived = investigation.HelpReceived.Any(o => o.FloodMitigationId != FloodMitigationIds.NoHelp),
                RegisteredWithFloodline = registeredWithFloodline,
                ReceivedFloodlineWarningInTime = registeredWithFloodline && investigation.WarningTimelyId == RecordStatusIds.Yes,
                ReceivedOtherWarnings = investigation.WarningReceivedId == RecordStatusIds.Yes && otherWarningSources.Count > 0,
            };
        }

        public InvestigationDto ToDto()
        {
            return new InvestigationDto
            {
                // Internal fields
                WhenWaterEnteredKnownId = investigation.WhenWaterEnteredKnownId,
                FloodInternalUtc = investigation.FloodInternalUtc,

                // Peak depth fields
                InvestigationPeakDepths = investigation.PeakDepths, 

                // History fields
                HistoryOfFloodingId = investigation.HistoryOfFloodingId,
                HistoryOfFloodingDetails = investigation.HistoryOfFloodingDetails,
                PropertyInsuredId = investigation.PropertyInsuredId,

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

                // Service impact fields, which are related flood impacts
                ServiceImpacts = [.. investigation.ServiceImpacts.Select(o => o.FloodImpactId)],

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
        internal Investigation ApplyInternalFields(InvestigationDto dto, bool isInternal)
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
        internal Investigation ApplyPeakDepth(InvestigationDto dto)
        {
            if(dto.InvestigationPeakDepths != null && dto.InvestigationPeakDepths.Count > 0)
            {
                return investigation with
                {
                    PeakDepths = dto.InvestigationPeakDepths,
                };
            }
            return investigation;
        }

        /// <summary>
        /// Applies floodline warnings to the investigation only if the floodline warning source is present.
        /// </summary>
        internal Investigation ApplyFloodlineWarnings(InvestigationDto dto)
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
}

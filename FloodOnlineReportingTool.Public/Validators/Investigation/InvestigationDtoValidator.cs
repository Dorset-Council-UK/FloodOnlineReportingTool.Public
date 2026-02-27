using FloodOnlineReportingTool.Database.Models.Flood;
using FloodOnlineReportingTool.Database.Models.Flood.FloodProblemIds;
using FloodOnlineReportingTool.Database.Models.Investigate;
using FloodOnlineReportingTool.Database.Models.Status;
using FloodOnlineReportingTool.Public.Models.Order;
using FluentValidation;

namespace FloodOnlineReportingTool.Public.Validators.Investigation;

#pragma warning disable MA0051 // Method is too long

public class InvestigationDtoValidator : AbstractValidator<InvestigationDto>
{
    public InvestigationDtoValidator()
    {
        // Water speed (FloodProblem's)
        RuleFor(dto => dto.WaterSpeedId)
            .NotEmpty()
            .WithMessage("Select how fast the water was moving.")
            .WithState(dto => InvestigationPages.Speed);
        RuleFor(dto => dto.BeginId)
            .NotEmpty()
            .WithMessage("Select how quickly the flooding started.")
            .WithState(dto => InvestigationPages.Speed);
        RuleFor(dto => dto.AppearanceId)
            .NotEmpty()
            .WithMessage("Select what the water looked like.")
            .WithState(dto => InvestigationPages.Speed);
        RuleFor(dto => dto.MoreAppearanceDetails)
            .MaximumLength(200)
            .WithMessage("More appearance details must be {MaxLength} characters or less")
            .WithState(dto => InvestigationPages.Speed);

        When((dto, context) => context.IsInternal, () =>
        {
            // Internal how / Water entry (FloodProblem's)
            RuleFor(dto => dto.Entries)
                .NotEmpty()
                .WithMessage("Select how the water entered the property")
                .WithState(dto => InvestigationPages.InternalHow);
            RuleFor(dto => dto.WaterEnteredOther)
                .NotEmpty()
                .WithState(dto => InvestigationPages.InternalHow)
                .WithMessage("Enter other details of how the water entered")
                .MaximumLength(100)
                .WithMessage("Water entered other details must be {MaxLength} characters or less")
                .When(dto => dto.Entries.Contains(FloodEntryIds.Other));

            // Internal when (RecordStatus)
            RuleFor(dto => dto.WhenWaterEnteredKnownId)
                .NotNull()
                .WithMessage("Select if you know when the water entered")
                .WithState(dto => InvestigationPages.InternalWhen);
            RuleFor(dto => dto.FloodInternalUtc)
                .NotNull()
                .WithMessage("Enter the date the water entered")
                .WithState(dto => InvestigationPages.InternalWhen)
                .When(dto => dto.WhenWaterEnteredKnownId == RecordStatusIds.Yes);
        });

        // Water destination (FloodProblem's)
        RuleFor(dto => dto.Destinations)
            .NotEmpty()
            .WithMessage("Select where the flood water was flowing")
            .WithState(dto => InvestigationPages.Destination);

        // Damaged vehicles (RecordStatus)
        RuleFor(dto => dto.WereVehiclesDamagedId)
            .NotEmpty()
            .WithMessage("Select if vehicles were damaged")
            .WithState(dto => InvestigationPages.Vehicles);
        RuleFor(dto => dto.NumberOfVehiclesDamaged)
            .NotEmpty()
            .WithMessage("Enter the number of vehicles damaged")
            .WithState(dto => InvestigationPages.Vehicles)
            .When(dto => dto.WereVehiclesDamagedId == RecordStatusIds.Yes);

        // Peak depth (RecordStatus)
        RuleFor(dto => dto.IsPeakDepthKnownId)
            .NotEmpty()
            .WithMessage("Select if you know how deep the water was")
            .WithState(dto => InvestigationPages.PeakDepth);
        RuleFor(dto => dto.PeakInsideCentimetres)
            .NotEmpty()
            .WithMessage("Enter the depth inside")
            .WithState(dto => InvestigationPages.PeakDepth)
            .When(dto => dto.IsPeakDepthKnownId == RecordStatusIds.Yes);
        RuleFor(dto => dto.PeakOutsideCentimetres)
            .NotEmpty()
            .WithMessage("Enter the depth outside")
            .WithState(dto => InvestigationPages.PeakDepth)
            .When(dto => dto.IsPeakDepthKnownId == RecordStatusIds.Yes);

        // Service impacts (FloodImpact's)
        RuleFor(dto => dto.ServiceImpacts)
            .NotEmpty()
            .WithMessage("Select if any services were impacted")
            .WithState(dto => InvestigationPages.ServiceImpact);

        // Community impacts (FloodImpact's)
        RuleFor(dto => dto.CommunityImpacts)
            .NotEmpty()
            .WithMessage("Select where the community was impacted")
            .WithState(dto => InvestigationPages.CommunityImpact);

        // Blockages
        RuleFor(dto => dto.HasKnownProblems)
            .NotEmpty()
            .WithMessage("Select if there are any problems, blockages, or recent repair works")
            .WithState(dto => InvestigationPages.Blockages);
        RuleFor(dto => dto.KnownProblemDetails)
            .NotEmpty()
            .WithMessage("Enter the problems, blockages, or recent repair works")
            .WithState(dto => InvestigationPages.Blockages)
            .MaximumLength(200)
            .WithMessage("Details of the problems, blockages, or recent repair works must be {MaxLength} characters or less")
            .When(dto => dto.HasKnownProblems == true);

        // Actions taken (FloodMitigation's)
        RuleFor(dto => dto.ActionsTaken)
            .NotEmpty()
            .WithMessage("Select what actions you took")
            .WithState(dto => InvestigationPages.ActionsTaken);
        RuleFor(dto => dto.OtherAction)
            .NotEmpty()
            .WithMessage("Enter the other actions you took")
            .WithState(dto => InvestigationPages.ActionsTaken)
            .MaximumLength(100)
            .WithMessage("Other actions must be {MaxLength} characters or less")
            .When(dto => dto.ActionsTaken.Contains(FloodMitigationIds.OtherAction));

        // Warnings - Help received (FloodMitigation's)
        RuleFor(dto => dto.HelpReceived)
            .NotEmpty()
            .WithMessage("Select what help you received")
            .WithState(dto => InvestigationPages.HelpReceived);

        // Warnings - Before the flooding (RecordStatus)
        RuleFor(dto => dto.FloodlineId)
            .NotEmpty()
            .WithMessage("Select if you are registered to receive floodline warnings")
            .WithState(dto => InvestigationPages.Warnings);
        RuleFor(dto => dto.WarningReceivedId)
            .NotEmpty()
            .WithMessage("Select if you got any other warnings")
            .WithState(dto => InvestigationPages.Warnings);

        // Warnings - Sources (FloodMitigation's)
        RuleFor(dto => dto.WarningSources)
            .NotEmpty()
            .WithMessage("Select where you received warnings from")
            .WithState(dto => InvestigationPages.WarningSources);
        RuleFor(dto => dto.WarningSourceOther)
            .NotEmpty()
            .WithMessage("Enter the other warning source")
            .WithState(dto => InvestigationPages.WarningSources)
            .MaximumLength(100)
            .WithMessage("Other warning source must be {MaxLength} characters or less")
            .When(dto => dto.WarningSources.Contains(FloodMitigationIds.OtherWarning));

        // Warnings - Floodline (RecordStatus)
        When(dto => dto.WarningSources.Contains(FloodMitigationIds.FloodlineWarning), () =>
        {
            RuleFor(dto => dto.WarningTimelyId)
                .NotEmpty()
                .WithMessage("Select if the floodline warning was timely")
                .WithState(dto => InvestigationPages.Floodline);
            RuleFor(dto => dto.WarningAppropriateId)
                .NotEmpty()
                .WithMessage("Select if the warning was worded correctly")
                .WithState(dto => InvestigationPages.Floodline);
        });

        // History (RecordStatus)
        RuleFor(dto => dto.HistoryOfFloodingId)
            .NotEmpty()
            .WithMessage("Select if you know if there is history of flooding")
            .WithState(dto => InvestigationPages.History);
        RuleFor(dto => dto.HistoryOfFloodingDetails)
            .NotEmpty()
            .WithMessage("Enter the history of the flooding")
            .WithState(dto => InvestigationPages.History)
            .MaximumLength(200)
            .WithMessage("History of flooding must be {MaxLength} characters or less")
            .When(dto => dto.HistoryOfFloodingId == RecordStatusIds.Yes);
        RuleFor(dto => dto.PropertyInsuredId)
            .NotEmpty()
            .WithMessage("Select if the property is insured")
            .WithState(dto => InvestigationPages.History);
    }
}

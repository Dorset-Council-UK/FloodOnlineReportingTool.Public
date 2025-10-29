using FloodOnlineReportingTool.Database.Models.Status;
using FloodOnlineReportingTool.Public.Models.FloodReport.Investigation;
using FluentValidation;

namespace FloodOnlineReportingTool.Public.Validators.Investigation;

public class SummaryValidator : AbstractValidator<Summary>
{
    public SummaryValidator()
    {
        ValidateWaterSpeed();
        ValidateDestination();
        ValidateVehicles();
        ValidateInternal();
        ValidatePeakDepth();
        ValidateCommunityImpact();
        ValidateBlockages();
        ValidateActionsTaken();
        ValidateHelpReceived();
        ValidateWarnings();
        ValidateWarningSources();
        ValidateFloodlineWarnings();
        ValidateHistory();
    }

    private void ValidateHistory()
    {
        RuleFor(o => o.HistoryOfFloodingLabel)
            .NotEmpty()
            .WithMessage("A history of flooding is required");
    }

    private void ValidateFloodlineWarnings()
    {
        When(o => o.IsFloodlineWarning, () =>
        {
            RuleFor(o => o.WarningTimelyLabel)
                .NotEmpty()
                .WithMessage("We need to know if the warning was timely. Please answer the questions on the floodline warning page");

            RuleFor(o => o.WarningAppropriateLabel)
                .NotEmpty()
                .WithMessage("We need to know if the warning was appropriate. Please answer the questions on the floodline warning page");
        });
    }

    private void ValidateWarningSources()
    {
        RuleFor(o => o.WarningSourceLabels)
            .NotEmpty()
            .WithMessage("Warning sources are required");
    }

    /// <summary>
    /// Before the flooding - Warnings
    /// </summary>
    private void ValidateWarnings()
    {
        RuleFor(o => o.RegisteredWithFloodlineLabel)
            .NotEmpty()
            .WithMessage("Knowing if you are registered to receive floodline warnings is required");

        RuleFor(o => o.OtherWarningReceivedLabel)
            .NotEmpty()
            .WithMessage("Knowing if you received any other warnings is required");
    }

    private void ValidateHelpReceived()
    {
        RuleFor(o => o.HelpReceivedLabels)
            .NotEmpty()
            .WithMessage("Help received is required");
    }

    private void ValidateActionsTaken()
    {
        RuleFor(o => o.ActionsTakenLabels)
            .NotEmpty()
            .WithMessage("Actions taken is required");
    }

    private void ValidateBlockages()
    {
        RuleFor(o => o.HasKnownProblemsMessage)
            .NotEmpty()
            .WithMessage("If there were known problems is required");
    }

    private void ValidateCommunityImpact()
    {
        RuleFor(o => o.CommunityImpactLabels)
            .NotEmpty()
            .WithMessage("Community impacts are required");
    }

    private void ValidatePeakDepth()
    {
        RuleFor(o => o.IsPeakDepthKnownId)
            .NotEmpty()
            .WithMessage("We need to know how deep the water was. Please answer the questions on the peak depth page");

        RuleFor(o => o.PeakDepthMessage)
            .NotEmpty()
            .WithMessage("We need to know how deep the water was. Please answer the questions on the peak depth page")
            .When(o => o.IsPeakDepthKnownId == RecordStatusIds.No);

        RuleFor(o => o.PeakDepthInsideMessage)
            .NotEmpty()
            .WithMessage("We need to know how deep the water was inside. Please answer the questions on the peak depth page")
            .When(o => o.IsPeakDepthKnownId == RecordStatusIds.Yes);

        RuleFor(o => o.PeakDepthOutsideMessage)
            .NotEmpty()
            .WithMessage("We need to know how deep the water was outside. Please answer the questions on the peak depth page")
            .When(o => o.IsPeakDepthKnownId == RecordStatusIds.Yes);
    }

    private void ValidateInternal()
    {
        When(o => o.IsInternal, () =>
        {
            RuleFor(o => o.EntryLabels)
                .NotEmpty()
                .WithMessage("How the water entered the property is required");

            RuleFor(o => o.InternalMessage)
                .NotEmpty()
                .WithMessage("How the water entered the property is required");
        });
    }

    private void ValidateVehicles()
    {
        RuleFor(o => o.VehiclesDamagedMessage)
            .NotEmpty()
            .WithMessage("Whether vehicles were damaged is required");
    }

    private void ValidateDestination()
    {
        RuleFor(o => o.DestinationLabels)
            .NotEmpty()
            .WithMessage("Water detinations are required");
    }

    private void ValidateWaterSpeed()
    {
        RuleFor(o => o.BeginLabel)
            .NotEmpty()
            .WithMessage("How quickly the flooding started is required");

        RuleFor(o => o.WaterSpeedLabel)
            .NotEmpty()
            .WithMessage("How fast the water was moving is required");

        RuleFor(o => o.AppearanceLabel)
            .NotEmpty()
            .WithMessage("What the water looked like is required");
    }
}

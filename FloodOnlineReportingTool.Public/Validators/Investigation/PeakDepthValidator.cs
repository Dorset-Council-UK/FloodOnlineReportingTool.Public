using FloodOnlineReportingTool.Database.Models.Status;
using FloodOnlineReportingTool.Public.Models.FloodReport.Investigation;
using FluentValidation;

namespace FloodOnlineReportingTool.Public.Validators.Investigation;

public class PeakDepthValidator : AbstractValidator<PeakDepth>
{
    public PeakDepthValidator()
    {
        RuleFor(o => o.IsPeakDepthKnownId)
            .NotEmpty()
            .WithMessage("Select if you know how deep the water was");

        When(o => o.IsPeakDepthKnownId == RecordStatusIds.Yes, () =>
        {
            // Inside centimetres
            RuleFor(o => o.PeakCentimetresText)
                .NotEmpty()
                .WithMessage("Enter the depth inside");

            When(o => !string.IsNullOrWhiteSpace(o.PeakCentimetresText), () =>
            {
                RuleFor(o => o.PeakCentimetresNumber)
                    .NotEmpty()
                    .WithMessage("Depth inside must be a number")
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("Depth must be a positive number")
                    .OverridePropertyName(o => o.PeakCentimetresText);

                RuleFor(o => o.PeakCentimetres)
                    .NotEmpty()
                    .WithMessage("Depth inside must be a whole number, like 5")
                    .When(o => o.PeakCentimetresNumber != null)
                    .OverridePropertyName(o => o.PeakCentimetresText);

            });

            // Outside centimetres
            RuleFor(o => o.DepthLocation)
                .NotEmpty()
                .WithMessage("Enter the depth location")
                .MaximumLength(200)
                .WithMessage("Depth location must be 200 characters or less");
        });
    }
}


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
            RuleFor(o => o.InsideCentimetresText)
                .NotEmpty()
                .WithMessage("Enter the depth inside");

            When(o => !string.IsNullOrWhiteSpace(o.InsideCentimetresText), () =>
            {
                RuleFor(o => o.InsideCentimetresNumber)
                    .NotEmpty()
                    .WithMessage("Depth inside must be a number")
                    .OverridePropertyName(o => o.InsideCentimetresText);

                RuleFor(o => o.InsideCentimetres)
                    .NotEmpty()
                    .WithMessage("Depth inside must be a whole number, like 5")
                    .When(o => o.InsideCentimetresNumber != null)
                    .OverridePropertyName(o => o.InsideCentimetresText);

            });

            // Outside centimetres
            RuleFor(o => o.OutsideCentimetresText)
                .NotEmpty()
                .WithMessage("Enter the depth outside");

            When(o => !string.IsNullOrWhiteSpace(o.OutsideCentimetresText), () =>
            {
                RuleFor(o => o.OutsideCentimetresNumber)
                .NotEmpty()
                .WithMessage("Depth outside must be a number")
                .OverridePropertyName(o => o.OutsideCentimetresText);

                RuleFor(o => o.OutsideCentimetres)
                    .NotEmpty()
                    .WithMessage("Depth outside must be a whole number, like 5")
                    .When(o => o.OutsideCentimetresNumber != null)
                    .OverridePropertyName(o => o.OutsideCentimetresText);
            });
        });
    }
}

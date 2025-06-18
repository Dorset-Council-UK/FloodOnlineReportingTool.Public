using FloodOnlineReportingTool.Public.Models.FloodReport.Investigation;
using FluentValidation;

namespace FloodOnlineReportingTool.Public.Validators.Investigation;

public class SpeedValidator : AbstractValidator<Speed>
{
    public SpeedValidator()
    {
        RuleFor(o => o.Begin)
            .NotEmpty()
            .WithMessage("Select how quickly the flooding started.");

        RuleFor(o => o.WaterSpeed)
            .NotEmpty()
            .WithMessage("Select how fast the water was moving.");

        RuleFor(o => o.Appearance)
            .NotEmpty()
            .WithMessage("Select what the water looked like.");

        // more details is optional but has a max length of 200
        RuleFor(o => o.MoreDetails)
            .MaximumLength(200)
            .WithMessage("More details must be {MaxLength} characters or less");
    }
}

using FloodOnlineReportingTool.Public.Models.FloodReport.Investigation;
using FluentValidation;

namespace FloodOnlineReportingTool.Public.Validators.Investigation;

public class FloodlineValidator : AbstractValidator<Floodline>
{
    public FloodlineValidator()
    {
        // Safety check
        RuleFor(o => o.WarningTimelyId)
            .NotEmpty()
            .WithMessage("Select if the floodline warning was timely");

        RuleFor(o => o.WarningAppropriateId)
           .NotEmpty()
           .WithMessage("Select if the warning was worded correctly");
    }
}

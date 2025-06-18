using FloodOnlineReportingTool.Public.Models.FloodReport.Investigation;
using FluentValidation;

namespace FloodOnlineReportingTool.Public.Validators.Investigation;

public class WarningsValidator : AbstractValidator<Warnings>
{
    public WarningsValidator()
    {
        RuleFor(o => o.RegisteredWithFloodlineId)
            .NotEmpty()
            .WithMessage("Select if you are registered to receive floodline warnings");

        RuleFor(o => o.OtherWarningId)
           .NotEmpty()
           .WithMessage("Select if you got any other warnings");
    }
}

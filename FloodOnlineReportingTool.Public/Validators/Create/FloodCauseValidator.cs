using FloodOnlineReportingTool.Public.Models.FloodReport.Create;
using FluentValidation;

namespace FloodOnlineReportingTool.Public.Validators.Create;

public class FloodCauseValidator : AbstractValidator<FloodCause>
{
    public FloodCauseValidator()
    {
        RuleFor(o => o.CauseOptions)
            .NotEmpty()
            .WithMessage("Select where the flooding came from or select 'Not sure'");
    }
}

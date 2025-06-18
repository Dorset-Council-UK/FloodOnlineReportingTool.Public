using FloodOnlineReportingTool.DataAccess.Models;
using FloodOnlineReportingTool.Public.Models.FloodReport.Investigation;
using FluentValidation;

namespace FloodOnlineReportingTool.Public.Validators.Investigation;

public class ActionsTakenValidator : AbstractValidator<ActionsTaken>
{
    public ActionsTakenValidator()
    {
        RuleFor(o => o.ActionsTakenOptions)
            .Must(o => o.Any(x => x.Selected))
            .WithMessage("Select what actions you took or select 'No action taken'");

        RuleFor(o => o.OtherAction)
            .NotEmpty()
            .WithMessage("Enter the other actions you took")
            .MaximumLength(100)
            .WithMessage("Other actions must be {MaxLength} characters or less")
            .When(entry => entry.ActionsTakenOptions.Any(option => option.Selected && option.Value.Equals(FloodMitigationIds.OtherAction)));
    }
}

using FloodOnlineReportingTool.Database.Models.Flood;
using FloodOnlineReportingTool.Public.Models.FloodReport.Investigation;
using FluentValidation;

namespace FloodOnlineReportingTool.Public.Validators.Investigation;

public class ActionsTakenValidator : AbstractValidator<ActionsTaken>
{
    public ActionsTakenValidator()
    {
        RuleFor(o => o.ActionsTakenOptions)
            .NotEmpty()
            .WithMessage("Select what actions you took or select 'No action taken'");

        RuleFor(o => o.OtherAction)
            .NotEmpty()
            .WithMessage("Enter the other actions you took")
            .MaximumLength(150)
            .WithMessage("Other actions must be {MaxLength} characters or less")
            .When(entry => entry.ActionsTakenOptions.Any(option => option.Equals(FloodMitigationIds.OtherAction)));
    }
}

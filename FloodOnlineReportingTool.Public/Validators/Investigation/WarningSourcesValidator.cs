using FloodOnlineReportingTool.DataAccess.Models;
using FloodOnlineReportingTool.Public.Models.FloodReport.Investigation;
using FluentValidation;

namespace FloodOnlineReportingTool.Public.Validators.Investigation;

public class WarningSourcesValidator : AbstractValidator<WarningSources>
{
    public WarningSourcesValidator()
    {
        RuleFor(o => o.WarningSourceOptions)
            .Must(o => o.Any(x => x.Selected))
            .WithMessage("Select where you received warnings from or select 'I did not get a warning'");

        RuleFor(o => o.WarningOther)
            .NotEmpty()
            .WithMessage("Enter the other warning source")
            .MaximumLength(100)
            .WithMessage("Other warning source must be {MaxLength} characters or less")
            .When(entry => entry.WarningSourceOptions.Any(option => option.Selected && option.Value.Equals(FloodMitigationIds.OtherWarning)));
    }
}

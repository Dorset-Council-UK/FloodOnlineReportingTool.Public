using FloodOnlineReportingTool.Database.Models;
using FloodOnlineReportingTool.Public.Models.FloodReport.Create;
using FluentValidation;

namespace FloodOnlineReportingTool.Public.Validators.Create;

public class FloodDurationValidator : AbstractValidator<FloodDuration>
{
    public FloodDurationValidator()
    {
        RuleFor(o => o.DurationKnownId)
            .NotEmpty()
            .WithMessage("Select the duration of the flooding");

        When(o => o.DurationKnownId == FloodProblemIds.DurationKnown, () =>
        {
            RuleFor(o => o.DurationDaysText)
                .NotEmpty()
                .WithMessage("Enter the flood duration in days or hours")
                .When(o => string.IsNullOrWhiteSpace(o.DurationHoursText));

            RuleFor(o => o.DurationDaysNumber)
                .NotEmpty()
                .WithMessage("Flood duration days must be a whole number like 3")
                .InclusiveBetween(0, 366)
                .WithMessage("Flood duration days has to be between {From} and {To}")
                .OverridePropertyName(o => o.DurationDaysText)
                .When(o => !string.IsNullOrWhiteSpace(o.DurationDaysText));

            RuleFor(o => o.DurationHoursText)
                .NotEmpty()
                .WithMessage("Enter the flood duration in days or hours")
                .When(o => string.IsNullOrWhiteSpace(o.DurationDaysText));

            RuleFor(o => o.DurationHoursNumber)
                .NotEmpty()
                .WithMessage("Flood duration hours must be a whole number like 3")
                .InclusiveBetween(0, 23)
                .WithMessage("Flood duration hours has to be between {From} and {To}")
                .OverridePropertyName(o => o.DurationHoursText)
                .When(o => !string.IsNullOrWhiteSpace(o.DurationHoursText));
        });
    }
}

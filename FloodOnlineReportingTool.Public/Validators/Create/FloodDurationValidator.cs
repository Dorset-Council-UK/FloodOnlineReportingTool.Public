using FloodOnlineReportingTool.DataAccess.Models;
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
            RuleFor(o => o.DurationDays)
                .Empty()
                .WithMessage("Enter the flood duration in days or hours")
                .InclusiveBetween(0, 366)
                .WithMessage("Flood duration days has to be between {From} and {To}");

            RuleFor(o => o.DurationHours)
                .Empty()
                .WithMessage("Enter the flood duration in days or hours")
                .InclusiveBetween(0, 23)
                .WithMessage("Flood duration hours has to between {From} and {To}");
        });
    }
}

using FloodOnlineReportingTool.Public.Models.FloodReport.Create;
using FloodOnlineReportingTool.Public.Validators.Gds;
using FluentValidation;
using GdsBlazorComponents.Validators;

namespace FloodOnlineReportingTool.Public.Validators.Create;

public class FloodStartedValidator : AbstractValidator<FloodStarted>
{
    public FloodStartedValidator()
    {
        var now = DateTimeOffset.UtcNow;

        //RuleFor(o => o.StartDate)
        //    .SetValidator(new GdsDateValidator("ORIGINAL TEST Flooding start"));

        RuleFor(o => o.StartDate)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MustBeNumber()
            .DaysInMonth()
            .YearInclusiveBetween(1900, now.Year + 1)
            .WithName("Flooding start date");

        // If the date is in the future when it needs to be today or in the past
        RuleFor(o => o.StartDate.DateUtc)
            .LessThanOrEqualTo(now)
            .WithMessage("Flooding start date must be today or in the past")
            .When(o => o.StartDate.DateUtc != null);

        RuleFor(x => x.IsFloodOngoing)
            .NotNull()
            .WithMessage("Confirm if the property is still flooded");
    }
}

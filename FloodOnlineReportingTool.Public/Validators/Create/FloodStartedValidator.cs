using FloodOnlineReportingTool.Public.Models.FloodReport.Create;
using FloodOnlineReportingTool.Public.Validators.Gds;
using FluentValidation;

namespace FloodOnlineReportingTool.Public.Validators.Create;

public class FloodStartedValidator : AbstractValidator<FloodStarted>
{
    public FloodStartedValidator()
    {
        var now = DateTimeOffset.UtcNow;

        RuleFor(o => o.StartDate)
            .SetValidator(new GdsDateValidator("Flooding start"));

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

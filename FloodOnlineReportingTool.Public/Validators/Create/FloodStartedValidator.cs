using FloodOnlineReportingTool.Public.Models.FloodReport.Create;
using FluentValidation;

namespace FloodOnlineReportingTool.Public.Validators.Create;

public class FloodStartedValidator : AbstractValidator<FloodStarted>
{
    public FloodStartedValidator()
    {
        var now = DateTimeOffset.UtcNow;

        RuleFor(o => o.StartDate)
            .Cascade(CascadeMode.Stop)
            .DayMonthYearNotEmpty()
            .DayMustBeNumber()
            .MonthMustBeNumber()
            .YearMustBeNumber()
            .DayInclusiveBetween(1, 31)
            .MonthInclusiveBetween(1, 12)
            .YearInclusiveBetween(1900, now.Year + 1)
            .CorrectDaysInMonth()
            .IsRealDate()
            .WithName("Flooding start date");

        // If the date is in the future when it needs to be today or in the past
        RuleFor(o => o.StartDate.DateUtc)
            .LessThanOrEqualTo(now)
            .WithMessage("Flooding start date must be today or in the past")
            .When(o => o.StartDate.DateUtc != null)
            .OverridePropertyName(o => o.StartDate);

        RuleFor(x => x.IsFloodOngoing)
            .NotNull()
            .WithMessage("Confirm if the property is still flooded");
    }
}

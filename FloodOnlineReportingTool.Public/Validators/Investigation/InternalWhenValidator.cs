using FloodOnlineReportingTool.Database.Models;
using FloodOnlineReportingTool.Public.Models.FloodReport.Investigation;
using FluentValidation;

namespace FloodOnlineReportingTool.Public.Validators.Investigation;

public class InternalWhenValidator : AbstractValidator<InternalWhen>
{
    public InternalWhenValidator()
    {
        var now = DateTimeOffset.UtcNow;

        RuleFor(o => o.WhenWaterEnteredKnownId)
            .NotEmpty()
            .WithMessage("Select if you know when the water entered");

        When(o => o.WhenWaterEnteredKnownId == RecordStatusIds.Yes, () =>
        {
            RuleFor(o => o.WhenWaterEnteredDate)
                .Cascade(CascadeMode.Stop)
                .DayMonthYearNotEmpty()
                .DayMustBeNumber()
                .MonthMustBeNumber()
                .YearMustBeNumber()
                .DayInclusiveBetween(1, 31)
                .MonthInclusiveBetween(1, 12)
                .YearInclusiveBetween(now.Year - 1, now.Year)
                .CorrectDaysInMonth()
                .IsRealDate()
                .WithName("When water entered");

            // If the date is in the future when it needs to be today or in the past
            RuleFor(o => o.WhenWaterEnteredDate.DateUtc)
                .LessThanOrEqualTo(now)
                .WithMessage("When water entered date must be today or in the past")
                .When(o => o.WhenWaterEnteredDate.DateUtc != null)
                .OverridePropertyName(o => o.WhenWaterEnteredDate);

            // If the date must be between two dates
            RuleFor(o => o.WhenWaterEnteredDate.DateUtc)
                .InclusiveBetween(now.AddYears(-1), now)
                .WithMessage((o, d) =>
                {
                    var from = now.AddYears(-1).GdsReadable();
                    var to = now.GdsReadable();
                    return $"When water entered date must be between {from} and {to}";
                })
                .When(o => o.WhenWaterEnteredDate.DateUtc != null && o.WhenWaterEnteredDate.DateUtc <= now)
                .OverridePropertyName(o => o.WhenWaterEnteredDate);

            RuleFor(o => o.Time)
                .NotEmpty()
                .WithMessage("The time the water entered must be a time")
                .OverridePropertyName(o => o.TimeText)
                .When(o => !string.IsNullOrWhiteSpace(o.TimeText) && !IsSpecialTimeText(o.TimeText));
        });
    }

    // Helper to check for special values, conversion of these special values to actual times in done in the pages OnSubmit
    private static bool IsSpecialTimeText(string? timeText)
    {
        var checkText = timeText?.Trim().ToLowerInvariant();
        return checkText is "morning" or "midday" or "noon" or "afternoon" or "midnight";
    }
}

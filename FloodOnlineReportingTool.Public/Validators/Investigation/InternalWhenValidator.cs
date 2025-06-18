using FloodOnlineReportingTool.DataAccess.Models;
using FloodOnlineReportingTool.Public.Models.FloodReport.Investigation;
using FloodOnlineReportingTool.Public.Validators.Gds;
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
                .SetValidator(new GdsDateValidator("When water entered"));

            // If the year entered cannot be correct
            RuleFor(o => o.WhenWaterEnteredDate.Year)
                .InclusiveBetween(now.Year - 1, now.Year)
                .WithMessage("When water entered year must be between {From} and {To}")
                .OverridePropertyName(o => o.WhenWaterEnteredDate.YearText);

            // If the date is in the future when it needs to be today or in the past
            RuleFor(o => o.WhenWaterEnteredDate.DateUtc)
                .LessThanOrEqualTo(now)
                .WithMessage("When water entered date must be today or in the past")
                .When(o => o.WhenWaterEnteredDate.DateUtc != null);

            // If the date must be between two dates
            RuleFor(o => o.WhenWaterEnteredDate.DateUtc)
                .InclusiveBetween(now.AddYears(-1), now)
                .WithMessage((o, d) => {
                    var from = now.AddYears(-1).GdsReadable();
                    var to = now.GdsReadable();
                    return $"When water entered date must be between {from} and {to}";
                })
                .When(o => o.WhenWaterEnteredDate.DateUtc != null && o.WhenWaterEnteredDate.DateUtc <= now);

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

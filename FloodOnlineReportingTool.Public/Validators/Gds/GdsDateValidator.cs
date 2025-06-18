using FloodOnlineReportingTool.GdsComponents;
using FluentValidation;
using System.Globalization;

namespace FloodOnlineReportingTool.Public.Validators.Gds;

/// <summary>
/// GDS quote: "If there's more than one error, show the highest priority error message." - https://design-system.service.gov.uk/components/date-input/
/// </summary>
/// <remarks>
/// Errors are added to the properties <c>DateUtc</c>, <c>DayText</c>, <c>MonthText</c>, and <c>YearText</c> as GDS recommends using input type=text fields for dates.
/// </remarks>
public class GdsDateValidator : AbstractValidator<GdsDate>
{
    private readonly string fieldName;

    public GdsDateValidator() : this("The")
    {
    }

    public GdsDateValidator(string name)
    {
        fieldName = name;

        // If nothing is entered
        RuleFor(o => o.DateUtc)
            .NotNull()
            .WithName(fieldName.ToLowerInvariant())
            .WithMessage("Enter a {PropertyName} date")
            .When(o => !IsPartDateTextEntered(o));

        // If the date is incomplete
        When(IsPartDateTextEntered, () =>
        {
            RuleFor(o => o.DayText)
                .NotEmpty()
                .WithName(fieldName)
                .WithMessage("{PropertyName} date must include a day");
            RuleFor(o => o.MonthText)
                .NotEmpty()
                .WithName(fieldName)
                .WithMessage("{PropertyName} date must include a month");
            RuleFor(o => o.YearText)
                .NotEmpty()
                .WithName(fieldName)
                .WithMessage("{PropertyName} date must include a year");
        });

        // If the day, month, or year cannot be correct
        When(IsAllDateTextEntered, () =>
        {
            // If the day entered cannot be correct
            RuleFor(o => o.Day)
                .NotEmpty()
                .WithName(fieldName)
                .WithMessage("{PropertyName} day must be a number between 1 and 31")
                .InclusiveBetween(1, 31)
                .WithMessage("{PropertyName} day must be between {From} and {To}")
                //.Must((o, day) => HasCorrectDaysInMonth(o))
                //.WithMessage((o, day) => $"{MonthName(o.Month!.Value)} does not have {day} days")
                .OverridePropertyName(o => o.DayText);

            // If the month entered cannot be correct
            RuleFor(o => o.Month)
                .NotEmpty()
                .WithName(fieldName)
                .WithMessage("{PropertyName} month must be a number between 1 and 12")
                .InclusiveBetween(1, 12)
                .WithMessage("{PropertyName} month must be between {From} and {To}")
                .OverridePropertyName(o => o.MonthText);

            // If the year entered cannot be correct
            RuleFor(o => o.Year)
                .NotEmpty()
                .WithName(fieldName)
                .WithMessage("{PropertyName} year must be a number")
                .OverridePropertyName(o => o.YearText);
        });

        // If the date entered cannot be correct
        When(o => o.Day != null && o.Month != null && o.Year != null, () =>
        {
            RuleFor(o => o.Day)
                .Must((o, day) => HasCorrectDaysInMonth(o))
                .WithName(fieldName)
                .WithMessage((o, day) =>
                {
                    var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(o.Month!.Value);
                    return $"{monthName} does not have {day} days";
                })
                .OverridePropertyName(o => o.DayText)
                .When(o => IsValidMonth(o.Month!.Value));

            RuleFor(o => o.DateUtc)
                .NotNull()
                .WithName(fieldName)
                .WithMessage("{PropertyName} date must be a real date")
                .When(HasCorrectDaysInMonth);
        });
    }

    private static bool IsPartDateTextEntered(GdsDate date)
    {
        return !string.IsNullOrWhiteSpace(date.DayText) || !string.IsNullOrWhiteSpace(date.MonthText) || !string.IsNullOrWhiteSpace(date.YearText);
    }

    private static bool IsAllDateTextEntered(GdsDate date)
    {
        return !string.IsNullOrWhiteSpace(date.DayText) && !string.IsNullOrWhiteSpace(date.MonthText) && !string.IsNullOrWhiteSpace(date.YearText);
    }

    private static bool IsValidMonth(int month)
    {
        return month is > 0 and <= 12;
    }

    private static bool HasCorrectDaysInMonth(GdsDate gdsDate)
    {
        if (gdsDate.Day == null || gdsDate.Month == null || gdsDate.Year == null)
        {
            return false;
        }

        int month = gdsDate.Month.Value;
        int year = gdsDate.Year.Value;

        if (!IsValidMonth(month))
        {
            return false;
        }

        if (year <= 0)
        {
            return false;
        }

        return gdsDate.Day.Value <= DateTime.DaysInMonth(gdsDate.Year.Value, gdsDate.Month.Value);
    }
}

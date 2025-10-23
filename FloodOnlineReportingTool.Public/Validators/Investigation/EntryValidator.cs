using FloodOnlineReportingTool.Public.Models.FloodReport.Investigation;
using FluentValidation;

namespace FloodOnlineReportingTool.Public.Validators.Investigation;

public class EntryValidator : AbstractValidator<Entry>
{
    public EntryValidator()
    {
        RuleFor(o => o.EntryOptions)
            .Must(o => o.Any(x => x.Selected))
            .WithMessage("Select how the water entered the property to or select 'Not sure'");

        RuleFor(o => o.WaterEnteredOther)
            .NotEmpty()
            .WithMessage("Enter other details of how the water entered")
            .MaximumLength(100)
            .WithMessage("Other details must be {MaxLength} characters or less")
            .When(entry => entry.EntryOptions.Any(option => option.Selected && option.Value.Equals(Database.Models.FloodProblemIds.FloodEntryIds.Other)));
    }
}

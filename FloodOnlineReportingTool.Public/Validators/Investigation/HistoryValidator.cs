
using FloodOnlineReportingTool.Database.Models.Status;
using FloodOnlineReportingTool.Public.Models.FloodReport.Investigation;
using FluentValidation;

namespace FloodOnlineReportingTool.Public.Validators.Investigation;

public class HistoryValidator : AbstractValidator<History>
{
    public HistoryValidator()
    {
        RuleFor(o => o.HistoryOfFloodingId)
             .NotEmpty()
             .WithMessage("Select if you know if there is history of flooding or select 'Not sure'");

        RuleFor(o => o.HistoryOfFloodingDetails)
            .NotEmpty()
            .WithMessage("Enter the history of the flooding")
            .MaximumLength(200)
            .WithMessage("History of flooding must be {MaxLength} characters or less")
            .When(o => o.HistoryOfFloodingId == RecordStatusIds.Yes);
    }
}
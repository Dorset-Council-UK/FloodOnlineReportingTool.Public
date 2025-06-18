using FloodOnlineReportingTool.Public.Models.FloodReport.Investigation;
using FluentValidation;

namespace FloodOnlineReportingTool.Public.Validators.Investigation;

public class FloodDestinationValidator : AbstractValidator<FloodDestination>
{
    public FloodDestinationValidator()
    {
        RuleFor(o => o.DestinationOptions)
            .Must(o => o.Any(x => x.Selected))
            .WithMessage("Select where the flood water was flowing to or select 'Not sure'");
    }
}

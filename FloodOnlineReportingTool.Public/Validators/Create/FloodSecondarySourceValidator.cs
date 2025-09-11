using FloodOnlineReportingTool.Public.Models.FloodReport.Create;
using FluentValidation;

namespace FloodOnlineReportingTool.Public.Validators.Create;

public class FloodSecondarySourceValidator : AbstractValidator<FloodSecondarySource>
{
    public FloodSecondarySourceValidator()
    {
        RuleFor(o => o.FloodSecondarySourceOptions)
            .Must(o => o.Any(x => x.Selected))
            .WithMessage("Select where the water was running off from or select 'Not sure'");
    }
}

using FloodOnlineReportingTool.Public.Models.FloodReport.Create;
using FluentValidation;

namespace FloodOnlineReportingTool.Public.Validators.Create;

public class FloodSourceValidator : AbstractValidator<FloodSource>
{
    public FloodSourceValidator()
    {
        RuleFor(o => o.FloodSourceOptions)
            .Must(o => o.Any(x => x.Selected))
            .WithMessage("Select where the flooding came from or select 'Not sure'");
    }
}

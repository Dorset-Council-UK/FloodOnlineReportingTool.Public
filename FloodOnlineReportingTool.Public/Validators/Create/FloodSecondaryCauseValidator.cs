using FloodOnlineReportingTool.Public.Models.FloodReport.Create;
using FluentValidation;

namespace FloodOnlineReportingTool.Public.Validators.Create;

public class FloodSecondaryCauseValidator : AbstractValidator<FloodSecondaryCause>
{
    public FloodSecondaryCauseValidator()
    {
        RuleFor(o => o.SecondaryCauseOptions)
            .Must(o => o.Any(item => item.Selected))
            .WithMessage("Select where the water was running off from or select 'Not sure'");
    }
}

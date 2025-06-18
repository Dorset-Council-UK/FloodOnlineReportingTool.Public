using FloodOnlineReportingTool.Public.Models.FloodReport.Investigation;
using FluentValidation;

namespace FloodOnlineReportingTool.Public.Validators.Investigation;

public class CommunityImpactValidator : AbstractValidator<CommunityImpact>
{
    public CommunityImpactValidator()
    {
        RuleFor(o => o.CommunityImpactOptions)
            .Must(o => o.Any(x => x.Selected))
            .WithMessage("Select where the community was impacted or select 'Not sure'");
    }
}

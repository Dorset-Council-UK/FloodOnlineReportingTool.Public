using FloodOnlineReportingTool.Public.Models.FloodReport.Investigation;
using FluentValidation;

namespace FloodOnlineReportingTool.Public.Validators.Investigation;

public class CommunityImpactValidator : AbstractValidator<CommunityImpact>
{
    public CommunityImpactValidator()
    {
        RuleFor(o => o.CommunityImpactOptions)
            .NotEmpty()
            .WithMessage("Select where the community was impacted or select 'Not sure'");
    }
}

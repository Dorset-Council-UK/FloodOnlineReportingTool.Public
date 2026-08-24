using FloodOnlineReportingTool.Database.Models.Status;
using FloodOnlineReportingTool.Public.Models.FloodReport.Investigation;
using FluentValidation;

namespace FloodOnlineReportingTool.Public.Validators.Investigation;

public class ServiceImpactValidator : AbstractValidator<ServiceImpact>
{
    public ServiceImpactValidator()
    {
        RuleFor(o => o.WereServicesImpactedId)
            .NotEmpty()
            .WithMessage("Select if any services were impacted");

        RuleFor(o => o.ImpactedServicesOptions)
            .NotEmpty()
            .WithMessage("Select which services were impacted")
            .When(o => o.WereServicesImpactedId == RecordStatusIds.Yes);
    }
}

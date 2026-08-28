using FloodOnlineReportingTool.Public.Models.FloodReport.Create;
using FluentValidation;

namespace FloodOnlineReportingTool.Public.Validators.Create;

public class FloodAreasValidator : AbstractValidator<FloodAreas>
{
    public FloodAreasValidator()
    {
        RuleFor(o => o.Residentials)
            .NotEmpty()
            .WithMessage("Select an area of the residential property that was flooded or select 'Not sure'")
            .When(o => o.ShowResidential);

        RuleFor(o => o.Commercials)
            .NotEmpty()
            .WithMessage("Select an area of the commercial property that was flooded or select 'Not sure'")
            .When(o => o.ShowCommercial);

        RuleFor(o => o.IsUninhabitable)
            .NotEmpty()
            .WithMessage("Confirm if the property was evacuated as a result of the flooding");
    }
}

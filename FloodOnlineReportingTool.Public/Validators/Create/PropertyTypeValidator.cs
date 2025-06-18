using FloodOnlineReportingTool.Public.Models.FloodReport.Create;
using FluentValidation;

namespace FloodOnlineReportingTool.Public.Validators.Create;

public class PropertyTypeValidator : AbstractValidator<PropertyType>
{
    public PropertyTypeValidator()
    {
        RuleFor(x => x.Property)
            .NotEmpty()
            .WithMessage("Select a property type")
            .When(x => x.PropertyOptions.Count > 0);

        RuleFor(o => o.PropertyOptions)
            .NotEmpty()
            .WithMessage("The property types are missing, please refresh and try again. If this message continues to appear please raise a bug report.")
            .OverridePropertyName(o => o.Property);

        RuleFor(x => x.ResponsibleOrganisations)
            .NotEmpty()
            .WithMessage("We were unable to work out which organistaions are responsible for this flood location. If this message continues to appear please raise a bug report.");
    }
}

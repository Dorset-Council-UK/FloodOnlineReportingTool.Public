using FloodOnlineReportingTool.Public.Models.FloodReport.Create;
using FluentValidation;

namespace FloodOnlineReportingTool.Public.Validators.Create;

public class PropertyTypeValidator : AbstractValidator<PropertyType>
{
    public PropertyTypeValidator()
    {
        RuleFor(x => x.Property)
            .NotEmpty()
            .WithMessage("Select a property type");

        RuleFor(x => x.ResponsibleOrganisations)
            .NotEmpty()
            .WithMessage("We were unable to work out which organistaions are responsible for this flood location. If this message continues to appear please raise a bug report.");
    }
}

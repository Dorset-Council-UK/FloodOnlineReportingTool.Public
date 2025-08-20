using FloodOnlineReportingTool.Public.Models.FloodReport.Create;
using FluentValidation;

namespace FloodOnlineReportingTool.Public.Validators.Create;

public class AddressValidator : AbstractValidator<Address>
{
    public AddressValidator()
    {
        RuleFor(o => o.UPRN)
            .NotEmpty()
            .WithMessage("Select the affected property")
            .When(o => o.AddressOptions.Count > 0)
            .When(o => o.IsAddress == true);

        RuleFor(o => o.Easting)
             .NotEmpty()
             .WithMessage("You need to select a location using the previous step")
             .When(o => o.IsAddress == false);

        RuleFor(o => o.Northing)
            .NotEmpty()
            .WithMessage("You need to select a location using the previous step")
            .When(o => o.IsAddress == false)
            .When(o => o.Easting.HasValue);

        RuleFor(o => o.LocationDesc)
            .NotEmpty()
            .WithMessage("You must describe the selected location or select an address")
            .When(o => o.IsAddress == false);
    }
}

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
            .When(o => !string.IsNullOrWhiteSpace(o.Postcode) && o.IsAddress);
    }
}

using FluentValidation;

namespace FloodOnlineReportingTool.Public.Validators.Create;

public class PostcodeValidator : AbstractValidator<Models.FloodReport.Create.SelectPostcode>
{
    public PostcodeValidator()
    {
        RuleFor(x => x.PostcodeKnown)
            .NotNull()
            .WithMessage("Select if this property has a postal address");

        RuleFor(x => x.Postcode)
            .NotEmpty()
            .WithMessage("A United Kingdom postcode is required if you know the address")
            .When(x => x?.PostcodeKnown == true);
    }
}

using FluentValidation;

namespace FloodOnlineReportingTool.Public.Validators.Create;

public class IndexValidator : AbstractValidator<Models.FloodReport.Create.Index>
{
    public IndexValidator()
    {
        RuleFor(x => x.IsAddress)
            .NotNull()
            .WithMessage("Select if this property has a postal address");
    }
}

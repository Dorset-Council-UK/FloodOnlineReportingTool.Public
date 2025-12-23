using FloodOnlineReportingTool.Public.Models.FloodReport.Contact.Subscribe;
using FluentValidation;

namespace FloodOnlineReportingTool.Public.Validators.Contacts;

public class VerifyValidator : AbstractValidator<VerifyModel>
{
    public VerifyValidator()
    {
        RuleFor(o => o.EnteredCodeText)
                .NotEmpty()
                .WithMessage("You need to provide the verification code you should have been emailed");

        RuleFor(o => o.EnteredCodeNumber)
                .NotEmpty()
                .WithMessage("The code must be a six digit number")
                .InclusiveBetween(100000, 999999)
                .WithMessage("You need to provide the six digit verification code you should have been emailed")
                .OverridePropertyName(o => o.EnteredCodeText)
                .When(o => !string.IsNullOrWhiteSpace(o.EnteredCodeText));
    }
}

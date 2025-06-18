using FloodOnlineReportingTool.Public.Models.Account;
using FluentValidation;

namespace FloodOnlineReportingTool.Public.Validators.Account;

public partial class SignInValidator : AbstractValidator<SignIn>
{
    public SignInValidator()
    {
        RuleFor(o => o.Email)
            .NotEmpty()
            .WithMessage("Enter your email address")
            .EmailAddress()
            .WithMessage("Enter a valid email address");

        // Example: january.dorset.teal
        RuleFor(o => o.Password)
            .NotEmpty()
            .WithMessage("Enter your password");
    }
}

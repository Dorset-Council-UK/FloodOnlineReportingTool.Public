using FloodOnlineReportingTool.Public.Models.Account;
using FluentValidation;

namespace FloodOnlineReportingTool.Public.Validators.Account;

public partial class RegisterValidator : AbstractValidator<Register>
{
    public RegisterValidator()
    {
        RuleFor(o => o.Email)
            .NotEmpty()
            .WithMessage("Enter your email address")
            .EmailAddress()
            .WithMessage("Enter a valid email address");

        RuleFor(o => o.Password)
            .NotEmpty()
            .WithMessage("Enter your password");

        RuleFor(o => o.ConfirmPassword)
            .NotEmpty()
            .WithMessage("Confirm your password")
            .Equal(o => o.Password)
            .WithMessage("Passwords do not match");
    }
}
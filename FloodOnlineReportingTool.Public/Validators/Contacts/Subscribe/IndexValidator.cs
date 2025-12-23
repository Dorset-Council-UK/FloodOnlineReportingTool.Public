using FloodOnlineReportingTool.Public.Models.FloodReport.Contact.Subscribe;
using FluentValidation;

namespace FloodOnlineReportingTool.Public.Validators.Contacts;

public class IndexValidator : AbstractValidator<SubscribeModel>
{
    public IndexValidator()
    {
        RuleFor(o => o.ContactName)
            .NotEmpty()
            .WithMessage("Enter your contact name");

        RuleFor(o => o.EmailAddress)
            .NotEmpty()
            .WithMessage("Enter your email address")
            .EmailAddress()
            .WithMessage("Enter an email address in the correct format, like name@example.com");

        RuleFor(o => o.PhoneNumber)
            .Must(PhoneNumberValidator.IsValid)
            .When(o => !string.IsNullOrWhiteSpace(o.PhoneNumber))
            .WithMessage(PhoneNumberValidator.ErrorMessage);
    }
}

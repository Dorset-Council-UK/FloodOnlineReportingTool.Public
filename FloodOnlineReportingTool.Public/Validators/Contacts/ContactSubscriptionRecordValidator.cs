using FloodOnlineReportingTool.Database.Models.Contact;
using FluentValidation;

namespace FloodOnlineReportingTool.Public.Validators.Contacts;

public class ContactSubscriptionRecordValidator : AbstractValidator<ContactSubscriptionRecord>
{
    public ContactSubscriptionRecordValidator()
    {
        RuleFor(o => o.ContactName)
            .NotEmpty()
            .WithMessage("Enter your contact name");

        RuleFor(o => o.EmailAddress)
            .NotEmpty()
            .WithMessage("Enter your email address")
            .EmailAddress()
            .WithMessage("Enter an email address in the correct format, like name@example.com");
    }
}

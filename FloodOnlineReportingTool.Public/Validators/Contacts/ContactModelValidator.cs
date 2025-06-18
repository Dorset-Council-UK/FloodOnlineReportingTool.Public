using FloodOnlineReportingTool.Public.Models.FloodReport.Contact;
using FluentValidation;

namespace FloodOnlineReportingTool.Public.Validators.Contacts;

public class ContactModelValidator : AbstractValidator<ContactModel>
{
    public ContactModelValidator()
    {
        RuleFor(o => o.ContactType)
            .IsInEnum()
            .WithMessage("The contact type is not valid")
            .NotEmpty()
            .WithMessage("Select the type of contact");

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

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
                .Must(BeValidPhoneNumber)
                .When(o => !string.IsNullOrWhiteSpace(o.PhoneNumber))
                .WithMessage("Enter a phone number in the correct format, like 01305 123459, 01632 960 001 or +447700 900982");
    }

    private bool BeValidPhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return true;

        // Remove spaces and hyphens to count digits
        var digitsOnly = new string(phoneNumber.Where(char.IsDigit).ToArray());

        // Must have at least 8 digits and 15 or less
        if (digitsOnly.Length < 8 || digitsOnly.Length > 15)
            return false;

        // Must contain only valid phone number characters
        if (!System.Text.RegularExpressions.Regex.IsMatch(phoneNumber, @"^[\+\d\s\-\(\)]+$"))
            return false;

        // The first digit (ignoring formatting characters) must be + or 0 or (
        var firstDigitOrPlus = phoneNumber.TrimStart().FirstOrDefault(c => char.IsDigit(c) || c == '+' || c == '(');
        return firstDigitOrPlus == '+' || firstDigitOrPlus == '0' || firstDigitOrPlus == '(';
    }
}

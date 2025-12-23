namespace FloodOnlineReportingTool.Public.Validators;

public static class PhoneNumberValidator
{
    /// <summary>
    /// Validates a phone number according to UK formatting rules.
    /// Must have 8-15 digits, start with +, 0, or (, and contain only valid characters.
    /// </summary>
    /// <param name="phoneNumber">The phone number to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValid(string? phoneNumber)
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

    /// <summary>
    /// Standard error message for invalid phone numbers
    /// </summary>
    public const string ErrorMessage = "Enter a phone number in the correct format, like 01305 123459, 01632 960 001 or +447700 900982";
}
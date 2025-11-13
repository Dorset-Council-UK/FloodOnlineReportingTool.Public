using System.Globalization;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System;
#pragma warning restore IDE0130 // Namespace does not match folder structure

internal static class TimeSpanExtensions
{
    internal static string GdsReadable(this TimeSpan? timeSpan)
    {
        if (timeSpan is null)
        {
            return string.Empty;
        }

        return GdsReadable(timeSpan.Value);
    }

    internal static string GdsReadable(this TimeSpan timeSpan)
    {
        if (timeSpan <= TimeSpan.Zero)
        {
            return string.Empty;
        }

        // Check for less than a minute remaining
        if (timeSpan.Days == 0 && timeSpan.Hours == 0 && timeSpan.Minutes == 0)
        {
            return (timeSpan.Seconds > 0) ? "less than a minute" : string.Empty;
        }

        // Build the list of time components, filtering out null entries
        // Only use the first two time parts
        var timeParts = new[]
        {
            FormatTimePart(timeSpan.Days, "day", "days"),
            FormatTimePart(timeSpan.Hours, "hour", "hours"),
            FormatTimePart(timeSpan.Minutes, "minute", "minutes"),
        }
        .Where(part => part is not null)
        .Take(2);

        return string.Join(' ', timeParts);
    }

    private static string? FormatTimePart(int number, string singular, string plural)
    {
        return number switch
        {
            0 => null,
            1 => string.Format(CultureInfo.CurrentCulture, "{0} {1}", number, singular),
            _ => string.Format(CultureInfo.CurrentCulture, "{0} {1}", number, plural),
        };
    }
}

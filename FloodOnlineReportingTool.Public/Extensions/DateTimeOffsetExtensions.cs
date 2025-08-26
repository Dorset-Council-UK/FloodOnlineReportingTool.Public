using System.Globalization;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System;
#pragma warning restore IDE0130 // Namespace does not match folder structure

internal static class DateTimeOffsetExtensions
{
    internal static string GdsReadable(this DateTimeOffset? dateTimeOffset)
    {
        return dateTimeOffset.GdsReadable(showYear: true);
    }

    internal static string GdsReadable(this DateTimeOffset? dateTimeOffset, bool showYear)
    {
        if (dateTimeOffset is null)
        {
            return string.Empty;
        }

        return GdsReadable(dateTimeOffset.Value, showYear);
    }

    internal static string GdsReadable(this DateTimeOffset dateTimeOffset)
    {
        return dateTimeOffset.GdsReadable(showYear: true);
    }

    internal static string GdsReadable(this DateTimeOffset dateTimeOffset, bool showYear)
    {

        if (showYear)
        {
            return dateTimeOffset.ToString("d MMMM yyyy", CultureInfo.CurrentCulture);
        }

        return dateTimeOffset.ToString("d MMMM", CultureInfo.CurrentCulture);
    }
}

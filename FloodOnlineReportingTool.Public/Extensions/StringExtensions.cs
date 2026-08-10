using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("Tests")]

namespace System;

internal static class StringExtensions
{
    /// <summary>
    /// Joins a collection of strings with commas (or custom separator) and "or" (or custom final separator) before the last item.
    /// </summary>
    /// <param name="items">The collection of strings to join</param>
    /// <param name="separator">The separator to use between strings. Defaults to ", " (with trailing space)</param>
    /// <param name="finalSeparator">The separator to use for the final item. Defaults to " or " with spaces around. If using a word, make sure you include spaces both sides.</param>
    /// <returns>A formatted string like "A, B or C"</returns>
    internal static string JoinWithOr(this IEnumerable<string> items, string separator = ", ", string finalSeparator = " or ")
    {
        var itemArray = items.ToArray();

        return itemArray.Length switch
        {
            0 => string.Empty,
            1 => itemArray[0],
            2 => $"{itemArray[0]}{finalSeparator}{itemArray[1]}",
            _ => $"{string.Join(separator, itemArray[..^1])}{finalSeparator}{itemArray[^1]}",
        };
    }


    public enum MeasurementDisplayType
    {
        Centimetres,
        MetresAndCentimetres,
        Inches,
        FeetAndInches
    }

    /// <summary>
    /// Converts a measurement in centimetres to a display string in the specified format.
    /// </summary>
    /// <param name="measurementInCentimetres">The measurement in centimetres to convert.</param>
    /// <param name="displayType">The format in which to display the measurement.</param>
    /// <returns>A string representing the measurement in the specified format.</returns>
    /// <example>When using MeasurementDisplayType.FeetAndInches, a value of 63 centimetres would be returned as "2 feet 1 inch".</example>
    internal static string ConvertMeasurementToDisplayString(this int? measurementInCentimetres, MeasurementDisplayType displayType)
    {
        if (measurementInCentimetres is null)
        {
            return string.Empty;
        }
        if (measurementInCentimetres < 0)
        {
            return "cannot convert negative number";
        }

        int measurementInInches = Convert.ToInt32(measurementInCentimetres.Value / 2.54);

        return displayType switch
        { 
            MeasurementDisplayType.Centimetres => 
                measurementInCentimetres switch
                {
                    1 => "1 centimetre",
                    _ => $"{measurementInCentimetres} centimetres"
                },
            MeasurementDisplayType.MetresAndCentimetres =>
                measurementInCentimetres switch
                {
                    1 => "1 centimetre",
                    < 100 => $"{measurementInCentimetres} centimetres",
                    100 => $"1 metre",
                    101 => $"1 metre 1 centimetre",
                    < 200 => $"1 metre {measurementInCentimetres - 100} centimetres",
                    _ when measurementInCentimetres % 100 == 0 => $"{measurementInCentimetres / 100} metres",
                    _ when measurementInCentimetres % 100 == 1 => $"{measurementInCentimetres / 100} metres 1 centimetre",
                    _ => $"{measurementInCentimetres / 100} metres {measurementInCentimetres % 100} centimetres"
                },
            MeasurementDisplayType.Inches =>
                measurementInInches switch
                {
                    1 => "1 inch",
                    _ => $"{measurementInInches} inches"
                },
            MeasurementDisplayType.FeetAndInches => 
                measurementInInches switch
                {
                    1 => "1 inch",
                    < 12 => $"{measurementInInches} inches",
                    12 => $"1 foot",
                    13 => $"1 foot 1 inch",
                    < 24 => $"1 foot {measurementInInches - 12} inches",
                    _ when measurementInInches % 12 == 0 => $"{measurementInInches / 12} feet",
                    _ when measurementInInches % 12 == 1 => $"{measurementInInches / 12} feet 1 inch",
                    _ => $"{measurementInInches / 12} feet {measurementInInches % 12} inches"
                },
            _ => string.Empty
        };   
    }
}
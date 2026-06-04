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
            2 => $"{itemArray[0]} {finalSeparator} {itemArray[1]}",
            _ => $"{string.Join(separator, itemArray[..^1])}{finalSeparator}{itemArray[^1]}",
        };
    }
}
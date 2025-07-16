namespace System;

internal static class StringExtensions
{
    /// <summary>
    /// Joins a collection of strings with commas (or cusotm seperator) and "or" (or custom final seperator) before the last item.
    /// </summary>
    /// <param name="items">The collection of strings to join</param>
    /// <param name="seperator">The seperator to use between strings. Defaults to ", " (with trailing space)</param>
    /// <param name="finalSeperator">The seperator to use for the final item. Defaults to " or " with spaces around. If using a word, make sure you include spaces both sides.</param>
    /// <returns>A formatted string like "A, B or C"</returns>
    internal static string JoinWithOr(this IEnumerable<string> items, string seperator = ", ", string finalSeperator = " or ")
    {
        var itemArray = items.ToArray();
        
        return itemArray.Length switch
        {
            0 => string.Empty,
            1 => itemArray[0],
            2 => $"{itemArray[0]} {finalSeperator} {itemArray[1]}",
            _ => $"{string.Join(seperator, itemArray[..^1])}{finalSeperator}{itemArray[^1]}",
        };
    }
}
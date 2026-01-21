using Microsoft.Extensions.Compliance.Redaction;

namespace FloodOnlineReportingTool.Database.Compliance;

public class StarRedactor : Redactor
{
    public override int Redact(ReadOnlySpan<char> source, Span<char> destination)
    {
        var length = Math.Min(source.Length, destination.Length);

        string convertedString = StarConverter(source.ToString());
        convertedString.CopyTo(destination);

        return length;
    }
    
    public override int GetRedactedLength(ReadOnlySpan<char> input)
    {
        return input.Length;
    }

    public static string StarConverter(string inputString, bool bookend = false)
    {
        int stringLength = inputString.Length;
        string redactedString = "";
        bool emailOverride = false;

        foreach (char c in inputString)
        {
            if (c == '@')
            {
                emailOverride = true;
            }
            if ((
                bookend && 
                (redactedString.Length == 0 || redactedString.Length == (stringLength - 1))
                ) || emailOverride)
            {
                redactedString += c;
            }
            else
            {
                redactedString += '*';
            }
        }

        return redactedString;
    }
}

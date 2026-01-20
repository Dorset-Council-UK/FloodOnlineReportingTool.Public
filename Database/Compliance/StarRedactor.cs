using Microsoft.Extensions.Compliance.Redaction;

namespace FloodOnlineReportingTool.Database.Compliance;

public class StarRedactor : Redactor
{
    public override int Redact(ReadOnlySpan<char> source, Span<char> destination)
    {
        var length = Math.Min(source.Length, destination.Length);
        
        // Check if this looks like an email address
        var atIndex = source.IndexOf('@');
        
        if (atIndex > 0 && atIndex < source.Length - 1)
        {
            // It's an email - redact only the local part (before @)
            destination.Slice(0, atIndex).Fill('*');
            
            // Copy the @ and domain part as-is
            source.Slice(atIndex).CopyTo(destination.Slice(atIndex));
        }
        else
        {
            // Not an email - redact everything
            destination.Slice(0, length).Fill('*');
        }
        
        return length;
    }
    
    public override int GetRedactedLength(ReadOnlySpan<char> input)
    {
        return input.Length;
    }
}

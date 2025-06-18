namespace FloodOnlineReportingTool.Public.Models;

public readonly record struct SaveResult(bool Failed, string ErrorMessage, string Reference)
{
    public static SaveResult Failure(ReadOnlySpan<char> errorMessage)
    {
        return new SaveResult(Failed: true, errorMessage.ToString(), Reference: string.Empty);
    }
    public static SaveResult Success(ReadOnlySpan<char> reference)
    {
        return new SaveResult(Failed: false, ErrorMessage: string.Empty, reference.ToString());
    }
}
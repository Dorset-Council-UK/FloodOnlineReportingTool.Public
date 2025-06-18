namespace FloodOnlineReportingTool.Public.Models;

public readonly record struct FortSignInResult(bool Succeeded, bool ShouldRedirect, string RedirectUrl, string ErrorMessage)
{
    public static FortSignInResult Success(ReadOnlySpan<char> redirectUrl)
    {
        return new FortSignInResult(Succeeded: true, ShouldRedirect: true, RedirectUrl: redirectUrl.ToString(), ErrorMessage: string.Empty);
    }
    public static FortSignInResult Redirect(ReadOnlySpan<char> redirectUrl)
    {
        return new FortSignInResult(Succeeded: false, ShouldRedirect: true, RedirectUrl: redirectUrl.ToString(), ErrorMessage: string.Empty);
    }
    public static FortSignInResult Error(ReadOnlySpan<char> errorMessage)
    {
        return new FortSignInResult(Succeeded: false, ShouldRedirect: false, RedirectUrl: string.Empty, ErrorMessage: errorMessage.ToString());
    }
}
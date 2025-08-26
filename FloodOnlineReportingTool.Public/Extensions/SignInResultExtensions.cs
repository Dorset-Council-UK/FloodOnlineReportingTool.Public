using FloodOnlineReportingTool.Public.Models;
using FloodOnlineReportingTool.Public.Models.Order;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.AspNetCore.Identity;
#pragma warning restore IDE0130 // Namespace does not match folder structure

internal static class SignInResultExtensions
{
    internal static FortSignInResult ToFortSignInResult(this SignInResult result, string? ReturnUrl, ILogger? logger = null)
    {
        if (result.Succeeded)
        {
            logger?.LogInformation("User signed in with password");
            return FortSignInResult.Success(ReturnUrl ?? GeneralPages.Home.Url);
        }

        if (result.IsLockedOut)
        {
            logger?.LogWarning("User is locked out");
            return FortSignInResult.Redirect(AccountPages.Lockout.Url);
        }

        if (result.IsNotAllowed)
        {
            logger?.LogError("User is not allowed to sign in");
            return FortSignInResult.Error("User is not allowed to sign in");
        }

        if (result.RequiresTwoFactor)
        {
            logger?.LogWarning("User requires two factor authentication");
            return FortSignInResult.Redirect(AccountPages.SignInWithTwoFactor.Url);
        }

        logger?.LogError("User sign in failed for an unknown reason");
        return FortSignInResult.Error("Unable to sign in, please try again");
    }
}

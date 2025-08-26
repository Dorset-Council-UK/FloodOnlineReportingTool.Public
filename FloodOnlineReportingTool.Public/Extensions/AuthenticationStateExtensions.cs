using System.Security.Claims;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.AspNetCore.Components.Authorization;
#pragma warning restore IDE0130 // Namespace does not match folder structure

internal static class AuthenticationStateExtensions
{
    internal static async Task<Guid?> IdentityUserId(this Task<AuthenticationState>? authenticationState)
    {
        if (authenticationState == null)
        {
            return null;
        }

        var authState = await authenticationState.ConfigureAwait(false);
        return authState?.User.IdentityUserId();
    }

    internal static Guid? IdentityUserId(this ClaimsPrincipal? user)
    {
        if (user?.Identity?.IsAuthenticated == true)
        {
            var nameidentifier = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(nameidentifier, out var userId))
            {
                return userId;
            }
        }

        return null;
    }
}

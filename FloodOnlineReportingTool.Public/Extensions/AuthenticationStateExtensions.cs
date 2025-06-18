using System.Security.Claims;

namespace Microsoft.AspNetCore.Components.Authorization;

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

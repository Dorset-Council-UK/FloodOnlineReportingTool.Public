using Microsoft.Identity.Web;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Security.Claims;
#pragma warning restore IDE0130 // Namespace does not match folder structure

internal static class ClaimsPrincipalExtensions
{
    private const string Unknown = "Unknown";

    private static string? GetClaimValue(ClaimsPrincipal? claimsPrincipal, params string[] claimNames)
    {
        if (claimsPrincipal is null)
        {
            return null;
        }

        for (var i = 0; i < claimNames.Length; i++)
        {
            var currentValue = claimsPrincipal.FindFirstValue(claimNames[i]);
            if (!string.IsNullOrWhiteSpace(currentValue))
            {
                return currentValue;
            }
        }

        return null;
    }

    /// <summary>
    /// Get the display name from the claims
    /// </summary>
    /// <remarks>
    ///     <para>GetDisplayName() from Microsoft.Identity.Web finds claims in the order preferred_username, http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name, name.</para>
    ///     <para>We need name to be more important.</para>
    /// </remarks>
    internal static string DisplayName(this ClaimsPrincipal? claimsPrincipal)
    {
        string[] claimNames = ["name", ClaimTypes.Name, "preferred_username"];
        return GetClaimValue(claimsPrincipal, claimNames) ?? Unknown;
    }

    /// <summary>
    /// Get the email address from the claims
    /// </summary>
    /// <remarks>Checking the claims in the order http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress, emails, preferred_username</remarks>
    internal static string Email(this ClaimsPrincipal? claimsPrincipal)
    {
        string[] claimNames = [ClaimTypes.Email, "emails", "preferred_username"];
        return GetClaimValue(claimsPrincipal, claimNames) ?? Unknown;
    }

    /// <summary>
    /// Get the identity provider from the claims
    /// </summary>
    /// <remarks>Checking the claims in the order http://schemas.microsoft.com/identity/claims/identityprovider, http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider, identityprovider, idp</remarks>
    internal static string IdentityProvider(this ClaimsPrincipal? claimsPrincipal)
    {
        string[] claimNames = [
            "http://schemas.microsoft.com/identity/claims/identityprovider",
            "http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider",
            "identityprovider",
            "idp",
        ];
        return GetClaimValue(claimsPrincipal, claimNames) ?? Unknown;
    }

    internal static bool IsAuthenticated(this ClaimsPrincipal? claimsPrincipal) =>
        claimsPrincipal?.Identity?.IsAuthenticated ?? false;

    /// <summary>
    /// Get the nameidentifier claim which is http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier
    /// </summary>
    internal static string UserId(this ClaimsPrincipal? claimsPrincipal) =>
        claimsPrincipal?.GetNameIdentifierId() ?? Unknown;

    /// <summary>
    /// Get the object identifier (oid) claim as a Guid from claim which is http://schemas.microsoft.com/identity/claims/objectidentifier
    /// </summary>
    internal static Guid UserOid (this ClaimsPrincipal? claimsPrincipal)
    {
        string[] claimNames = [
            "http://schemas.microsoft.com/identity/claims/objectidentifier",
        ];
        var oidClaim = GetClaimValue(claimsPrincipal, claimNames) ?? "";
        return Guid.TryParse(oidClaim, out var parsedOid) ? parsedOid : Guid.Empty;
    }
}

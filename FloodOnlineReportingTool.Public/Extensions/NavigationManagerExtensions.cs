namespace Microsoft.AspNetCore.Components;

internal static class NavigationManagerExtensions
{
    /// <summary>
    /// Determines whether the specified path represents an authentication-related flow.
    /// </summary>
    /// <remarks>The method checks for common authentication-related path prefixes, including 'signin', 'signout', 'signedout', and their 'account/' variants.</remarks>
    /// <returns>true if the path starts with a recognized authentication flow segment; otherwise, false.</returns>
    private static bool IsAuthenticationFlowPath(ReadOnlySpan<char> relativePath)
    {
        ReadOnlySpan<string> authPaths = [
            "signin",
            "signout",
            "signedout",
            "account/signin",
            "account/signout",
            "account/signedout",
        ];

        foreach (var authPath in authPaths)
        {
            if (relativePath.StartsWith(authPath, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    extension(NavigationManager navigationManager)
    {
        /// <summary>
        /// Get the redirect URI for sign-in flows, ensuring it is properly escaped and does not lead to authentication-related loops.
        /// </summary>
        internal string SignInRedirectUri
        {
            get
            {
                // ToBaseRelativePath strips the scheme, host, and base path
                // e.g., https://localhost:7039/report-flooding/floodreport -> floodreport
                var relativePath = navigationManager.ToBaseRelativePath(navigationManager.Uri);

                // Don't redirect to authentication-related paths to avoid loops
                if (string.IsNullOrWhiteSpace(relativePath) || IsAuthenticationFlowPath(relativePath))
                {
                    return string.Empty;
                }

                return Uri.EscapeDataString(relativePath);
            }
        }
    }
}

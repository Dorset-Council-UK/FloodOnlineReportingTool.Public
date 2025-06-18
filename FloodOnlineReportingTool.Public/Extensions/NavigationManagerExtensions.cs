namespace Microsoft.AspNetCore.Components;

internal static class NavigationManagerExtensions
{
    /// <summary>
    /// Checks if the redirect URL is local to the application.
    /// </summary>
    internal static bool IsLocalPath(this NavigationManager navigationManager, string redirectUrl)
    {
        var baseUri = new Uri(navigationManager.BaseUri, UriKind.Absolute);
        var relativeUri = new Uri(baseUri, redirectUrl);
        return relativeUri.AbsoluteUri.AsSpan().StartsWith(navigationManager.BaseUri.AsSpan(), StringComparison.Ordinal);
    }

    /// <summary>
    /// Converts a redirect URL to a local application path.
    /// </summary>
    internal static string LocalPathAndQuery(this NavigationManager navigationManager, string redirectUrl)
    {
        var baseUri = new Uri(navigationManager.BaseUri, UriKind.Absolute);
        var relativeUri = new Uri(baseUri, redirectUrl);

        if (relativeUri.AbsoluteUri.AsSpan().StartsWith(navigationManager.BaseUri.AsSpan(), StringComparison.Ordinal))
        {
            return relativeUri.PathAndQuery;
        }

        return "/";
    }
    /// <summary>
    /// Navigate to a local application path.
    /// </summary>
    internal static void NavigateToLocal(this NavigationManager navigationManager, string redirectUrl, bool forceLoad = false)
    {
        var uri = navigationManager.LocalPathAndQuery(redirectUrl);
        navigationManager.NavigateTo(uri, forceLoad);
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace FloodOnlineReportingTool.Public.Components.Pages.Account;

[Authorize]
public partial class SignIn(NavigationManager navigationManager)
{
    [CascadingParameter]
    public Task<AuthenticationState>? AuthenticationState { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (AuthenticationState is not null)
        {
            var authState = await AuthenticationState;
            if (authState.User.Identity?.IsAuthenticated == true)
            {
                // Get the return URL from query string
                var uri = new Uri(navigationManager.Uri);
                var returnUrl = System.Web.HttpUtility.ParseQueryString(uri.Query).Get("returnUrl");

                // Redirect to original page or fallback
                navigationManager.NavigateTo(returnUrl ?? "/", forceLoad: true);
            }
        }
    }

}
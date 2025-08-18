using FloodOnlineReportingTool.Database.Models;
using FloodOnlineReportingTool.Public.Models.Order;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;

namespace FloodOnlineReportingTool.Public.Components.Pages.Account;

// Static server-side rendering (static SSR)
[ExcludeFromInteractiveRouting]
public partial class SignOut(
    ILogger<SignOut> logger,
    SignInManager<FortUser> signInManager
) : IPageOrder
{
    // Page order properties
    public string Title { get; set; } = AccountPages.SignOut.Title;
    public IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [];

    protected override async Task OnInitializedAsync()
    {
        if (signInManager.IsSignedIn(signInManager.Context.User))
        {
            logger.LogInformation("Signing out the user");
            await signInManager.SignOutAsync();

            // Note:
            // Because of the static SSR needed for Identity SignOut, we cannot use JSInterop to clear the storage
            // And if we use navigationManager.NavigateTo in SSR it will throw a navigation exception meaning the JavaScript will not be loaded
            // So clearing and redirecting in this case is handled by a normal JavaScript file
        }
    }
}

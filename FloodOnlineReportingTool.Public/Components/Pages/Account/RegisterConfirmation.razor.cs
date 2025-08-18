using FloodOnlineReportingTool.Database.Models;
using FloodOnlineReportingTool.Public.Models.Order;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;

namespace FloodOnlineReportingTool.Public.Components.Pages.Account;

public partial class RegisterConfirmation(
    ILogger<RegisterConfirmation> logger,
    UserManager<FortUser> userManager,
    NavigationManager navigationManager,
    IGdsJsInterop gdsJs
) : IPageOrder
{
    // Page order properties
    public string Title { get; set; } = AccountPages.RegisterConfirmation.Title;
    public IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [];

    [SupplyParameterFromQuery]
    private string? UserId { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (string.IsNullOrEmpty(UserId))
        {
            logger.LogError("UserId is missing");
            navigationManager.NavigateTo(GeneralPages.Home.Url);
            return;
        }

        var user = await userManager.FindByIdAsync(UserId);
        if (user == null)
        {
            logger.LogError("User {UserId} not found", UserId);
            navigationManager.NavigateTo(GeneralPages.AccessDenied.Url);
            return;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await gdsJs.InitGds();
        }
    }
}

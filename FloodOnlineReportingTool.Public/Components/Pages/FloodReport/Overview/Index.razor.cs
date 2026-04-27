using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.Public.Authentication;
using FloodOnlineReportingTool.Public.Models.Order;
using FloodOnlineReportingTool.Public.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport.Overview;

public partial class Index(
    IFloodReportRepository floodReportRepository,
    IAuthorizationService authorizationService,
    NavigationManager navigationManager,
    SessionStateService scopedSessionStorage
) : IAsyncDisposable
{
    [CascadingParameter]
    public Task<AuthenticationState>? AuthenticationState { get; set; }

    // Text which changes for users and admins
    private string _h1TitleText = FloodReportPages.Overview.Title;
    private string _manageText = "Manage your flood reports";
    private string _noneFoundText = "We cannot find your flood reports.";
    private string _tableCaption = "Your flood reports";

    private readonly CancellationTokenSource _cts = new();
    private bool _isLoading = true;
    private IReadOnlyCollection<Database.Models.Flood.FloodReport> _floodReports = [];
    private readonly string _signInUrl = string.IsNullOrWhiteSpace(navigationManager.SignInRedirectUri)
        ? AccountPages.SignIn.Url
        : $"{AccountPages.SignIn.Url}?redirectUri={navigationManager.SignInRedirectUri}";

    public async ValueTask DisposeAsync()
    {
        try
        {
            await _cts.CancelAsync();
            _cts.Dispose();
        }
        catch (Exception)
        {
        }

        GC.SuppressFinalize(this);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            /*
             * scenarios to handle:
             * Not authenticated
             * Not authenticated with flood report ID in protected session storage
             * Authenticated as an admin
             * Authenticated as a user
             */

            if (AuthenticationState is not null)
            {
                var authState = await AuthenticationState;
                if (authState.User.IsAuthenticated)
                {
                    var adminPolicyCheck = await authorizationService.AuthorizeAsync(authState.User, PolicyNames.Admin);
                    var hasAdminPolicy = adminPolicyCheck.Succeeded;

                    _floodReports = hasAdminPolicy
                        ? await GetAdminsFloodReports(authState, hasAdminPolicy)
                        : await GetCurrentUsersFloodReports(authState, hasAdminPolicy);
                }
                else
                {
                    _floodReports = await GetStoredFloodReports(authState);
                }
            }

            _isLoading = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Gets all flood reports
    /// </summary>
    /// <remarks>The user has to be authenticated, and an admin.</remarks>
    private async Task<IReadOnlyCollection<Database.Models.Flood.FloodReport>> GetAdminsFloodReports(AuthenticationState authState, bool hasAdminPolicy)
    {
        if (!authState.User.IsAuthenticated || !hasAdminPolicy)
        {
            return [];
        }

        // Update the title, manage text, and none found text for admins
        _h1TitleText = "View all flood reports";
        _manageText = "Manage all flood reports";
        _noneFoundText = "We cannot find any flood reports.";
        _tableCaption = "All flood reports";

        return await floodReportRepository.GetAllOverview(_cts.Token);
    }

    /// <summary>
    /// Get the current users floods reports
    /// </summary>
    /// <remarks>The user has to be authenticated, and NOT an admin.</remarks>
    private async Task<IReadOnlyCollection<Database.Models.Flood.FloodReport>> GetCurrentUsersFloodReports(AuthenticationState authState, bool hasAdminPolicy)
    {
        if (!authState.User.IsAuthenticated || hasAdminPolicy)
        {
            return [];
        }

        string? userId = authState.User.Oid;
        if (userId is null)
        {
            return [];
        }

        return await floodReportRepository.AllReportedByContact(userId, _cts.Token);
    }

    /// <summary>
    /// Get the stored flood report
    /// </summary>
    /// <remarks>
    ///     <para>The user has to not be authenticated. Flood report ID exists in protected storage.</para>
    ///     <para>This can happen from the /floodreport/create/confirmation page</para>
    /// </remarks>
    private async Task<IReadOnlyCollection<Database.Models.Flood.FloodReport>> GetStoredFloodReports(AuthenticationState authState)
    {
        if (authState.User.IsAuthenticated)
        {
            return [];
        }

        var floodReportId = await scopedSessionStorage.GetFloodReportId();
        if (floodReportId == Guid.Empty)
        {
            return [];
        }

        var floodReport = await floodReportRepository.GetById(floodReportId, _cts.Token);
        if (floodReport is null)
        {
            return [];
        }

        return [floodReport];
    }
}

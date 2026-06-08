using FloodOnlineReportingTool.Database.Models.Eligibility;
using FloodOnlineReportingTool.Database.Models.Flood;
using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.Public.Authentication;
using FloodOnlineReportingTool.Public.Models.Order;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport.Overview;

public partial class OverviewDetailsAdmin(
    IFloodReportSourceRepository floodReportSourceRepository,
    IAuthorizationService authorizationService,
    NavigationManager navigationManager
) : IAsyncDisposable
{
    [CascadingParameter]
    public Task<AuthenticationState>? AuthenticationState { get; set; }

    [Parameter]
    public Guid? FloodReportSourceId { get; set; }

    [Parameter]
    public string? FloodReportSourceReference { get; set; }

    private readonly CancellationTokenSource _cts = new();
    private FloodReportSource? _floodReportSource;
    private EligibilityResult? _eligibilityResult;
    private bool _usersAccessHasExpired = true;
    private TimeSpan? _usersAccessTimeLeft;
    private readonly string _signInUrl = string.IsNullOrWhiteSpace(navigationManager.SignInRedirectUri)
        ? AccountPages.SignIn.Url
        : $"{AccountPages.SignIn.Url}?redirectUri={navigationManager.SignInRedirectUri}";
    private bool _initialised;

    public async ValueTask DisposeAsync()
    {
        try
        {
            await _cts.CancelAsync();
            _cts.Dispose();
        }
        catch (Exception)
        {
            // Ignore any exceptions that occur during disposal
        }

        GC.SuppressFinalize(this);
    }

    protected override async Task OnInitializedAsync()
    {
        if (AuthenticationState is not null)
        {
            var authState = await AuthenticationState;
            if (authState.User.IsAuthenticated)
            {
                var adminPolicyCheck = await authorizationService.AuthorizeAsync(authState.User, PolicyNames.Admin);
                var hasAdminPolicy = adminPolicyCheck.Succeeded;

                if (hasAdminPolicy)
                {
                    if (FloodReportSourceId is not null)
                    {
                        // Can we get the flood report source by ID
                        _floodReportSource = await floodReportSourceRepository.GetById(FloodReportSourceId.Value, _cts.Token);
                    }
                    else if (!string.IsNullOrWhiteSpace(FloodReportSourceReference))
                    {
                        // Can we get the flood report source by reference
                        _floodReportSource = await floodReportSourceRepository.GetByReference(FloodReportSourceReference, _cts.Token);
                    }

                    if (_floodReportSource is not null)
                    {
                        // Check if the users access has expired
                        if (_floodReportSource.ReportOwnerAccessUntil is not null)
                        {
                            _usersAccessTimeLeft = _floodReportSource.ReportOwnerAccessUntil.Value - DateTimeOffset.UtcNow;
                            _usersAccessHasExpired = _usersAccessTimeLeft <= TimeSpan.Zero;
                        }

                        _eligibilityResult = await floodReportSourceRepository.CalculateEligibilityWithReference(_floodReportSource.Reference, _cts.Token);
                    }
                }
            }
        }

        _initialised = true;
    }
}

using FloodOnlineReportingTool.Database.Models.Eligibility;
using FloodOnlineReportingTool.Database.Models.Flood;
using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.Public.Models.Order;
using FloodOnlineReportingTool.Public.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport.Overview;

public partial class OverviewDetailsAnonymous(
    IFloodReportSourceRepository floodReportSourceRepository,
    NavigationManager navigationManager,
    SessionStateService sessionStateService
) : IAsyncDisposable
{
    [CascadingParameter]
    public Task<AuthenticationState>? AuthenticationState { get; set; }

    private readonly CancellationTokenSource _cts = new();
    private FloodReportSource? _floodReportSource;
    private EligibilityResult? _eligibilityResult;
    private bool _accessHasExpired = true;
    private TimeSpan? _accessTimeLeft;
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
            if (!authState.User.IsAuthenticated)
            {
                var floodReportSourceId = await sessionStateService.GetFloodReportSourceId();
                if (floodReportSourceId != Guid.Empty)
                {
                    // Can we get the stored flood report source by ID
                    _floodReportSource = await floodReportSourceRepository.GetById(floodReportSourceId, _cts.Token);

                    if (_floodReportSource is not null)
                    {
                        // Check if the users access has expired
                        if (_floodReportSource.ReportOwnerAccessUntil is not null)
                        {
                            _accessTimeLeft = _floodReportSource.ReportOwnerAccessUntil.Value - DateTimeOffset.UtcNow;
                            _accessHasExpired = _accessTimeLeft <= TimeSpan.Zero;
                        }

                        _eligibilityResult = await floodReportSourceRepository.CalculateEligibilityWithReference(_floodReportSource.Reference, _cts.Token);
                    }
                }
            }
        }

        _initialised = true;
    }
}

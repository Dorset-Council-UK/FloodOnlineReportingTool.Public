using FloodOnlineReportingTool.Database.Models.Eligibility;
using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.Public.Models.Order;
using FloodOnlineReportingTool.Public.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport.Overview;

public partial class OverviewDetailsAnonymous(
    IFloodReportRepository floodReportRepository,
    NavigationManager navigationManager,
    SessionStateService sessionStateService
) : IAsyncDisposable
{
    [CascadingParameter]
    public Task<AuthenticationState>? AuthenticationState { get; set; }

    private readonly CancellationTokenSource _cts = new();
    private Database.Models.Flood.FloodReport? _floodReport;
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
                var floodReportId = await sessionStateService.GetFloodReportId();
                if (floodReportId != Guid.Empty)
                {
                    // Can we get the stored flood report by ID
                    _floodReport = await floodReportRepository.GetById(floodReportId, _cts.Token);

                    if (_floodReport is not null)
                    {
                        // Check if the users access has expired
                        if (_floodReport.ReportOwnerAccessUntil is not null)
                        {
                            _accessTimeLeft = _floodReport.ReportOwnerAccessUntil.Value - DateTimeOffset.UtcNow;
                            _accessHasExpired = _accessTimeLeft <= TimeSpan.Zero;
                        }

                        _eligibilityResult = await floodReportRepository.CalculateEligibilityWithReference(_floodReport.Reference, _cts.Token);
                    }
                }
            }
        }

        _initialised = true;
    }
}

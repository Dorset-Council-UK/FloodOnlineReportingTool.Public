using FloodOnlineReportingTool.Database.Models.Eligibility;
using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.Public.Authentication;
using FloodOnlineReportingTool.Public.Models.Order;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport.Overview;

public partial class OverviewDetailsAdmin(
    IFloodReportRepository floodReportRepository,
    IAuthorizationService authorizationService,
    NavigationManager navigationManager
) : IAsyncDisposable
{
    [CascadingParameter]
    public Task<AuthenticationState>? AuthenticationState { get; set; }

    [Parameter]
    public Guid? FloodReportId { get; set; }

    [Parameter]
    public string? FloodReportReference { get; set; }

    private readonly CancellationTokenSource _cts = new();
    private Database.Models.Flood.FloodReport? _floodReport;
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
                    if (FloodReportId is not null)
                    {
                        // Can we get the flood report by ID
                        _floodReport = await floodReportRepository.GetById(FloodReportId.Value, _cts.Token);
                    }
                    else if (!string.IsNullOrWhiteSpace(FloodReportReference))
                    {
                        // Can we get the flood report by reference
                        _floodReport = await floodReportRepository.GetByReference(FloodReportReference, _cts.Token);
                    }

                    if (_floodReport is not null)
                    {
                        // Check if the users access has expired
                        if (_floodReport.ReportOwnerAccessUntil is not null)
                        {
                            _usersAccessTimeLeft = _floodReport.ReportOwnerAccessUntil.Value - DateTimeOffset.UtcNow;
                            _usersAccessHasExpired = _usersAccessTimeLeft <= TimeSpan.Zero;
                        }

                        _eligibilityResult = await floodReportRepository.CalculateEligibilityWithReference(_floodReport.Reference, _cts.Token);
                    }
                }
            }
        }

        _initialised = true;
    }
}

using FloodOnlineReportingTool.Contracts.Shared;
using FloodOnlineReportingTool.Database.Models.Eligibility;
using FloodOnlineReportingTool.Database.Models.Responsibilities;
using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.Public.Models.Order;
using FloodOnlineReportingTool.Public.Services;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using static MassTransit.ValidationResultExtensions;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport;
public partial class Overview(
    IFloodReportRepository floodReportRepository,
    SessionStateService scopedSessionStorage,
    IGdsJsInterop gdsJs
) : IPageOrder, IAsyncDisposable
{
    // Page order properties
    public string Title { get; set; } = FloodReportPages.Overview.Title;
    public IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [GeneralPages.Home.ToGdsBreadcrumb()];

    [CascadingParameter]
    public Task<AuthenticationState>? AuthenticationState { get; set; }

    private readonly CancellationTokenSource _cts = new();
    private Guid _userId;
    private Guid _floodReportId = Guid.Empty;
    private bool _isLoading = true;
    private Database.Models.Flood.FloodReport? _floodReport;
    private bool _accessHasExpired = true;
    private TimeSpan _accessTimeLeft;

    // These are returning information about the current record
    private EligibilityOptions _floodInvestigation;
    private IList<Organisation> _leadLocalFloodAuthorities = [];
    private IList<Organisation> _otherFloodAuthorities = [];

    // These don't have any logic yet in the repository
    private bool _isEmergencyResponse;
    private string? _section19Url;
    private EligibilityOptions _grantApplication;
    private EligibilityOptions _propertyProtection;
    private EligibilityOptions _section19;

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

    protected override async Task OnInitializedAsync()
    {
        if (AuthenticationState == null)
        {
            return;
        }

        // Get the user's ID and check if they have the admin role
        var authState = await AuthenticationState;
        if (authState == null)
        {
            return;
        }

        _userId = authState.User.IdentityUserId() ?? Guid.Empty;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _floodReportId = await scopedSessionStorage.GetFloodReportId();
            _floodReport = await floodReportRepository.GetById(_floodReportId, _cts.Token);
            if (_floodReport != null)
            {
                var result = await floodReportRepository.CalculateEligibilityWithReference(_floodReport.Reference, _cts.Token);

                _leadLocalFloodAuthorities = [.. result.ResponsibleOrganisations.Where(o => o.FloodAuthorityId == FloodAuthorityIds.LeadLocalFloodAuthority)];
                _otherFloodAuthorities = [.. result.ResponsibleOrganisations.Where(o => o.FloodAuthorityId != FloodAuthorityIds.LeadLocalFloodAuthority)];

                // These don't have any logic yet in the repository
                _floodInvestigation = result.FloodInvestigation;
                _isEmergencyResponse = result.IsEmergencyResponse;
                _section19Url = result.Section19Url;
                _section19 = result.Section19;
                _propertyProtection = result.PropertyProtection;
                _grantApplication = result.GrantApplication;

                // Check if the users access has expired
                if (_floodReport.ReportOwnerAccessUntil != null)
                {
                    _accessTimeLeft = _floodReport.ReportOwnerAccessUntil.Value - DateTimeOffset.UtcNow;
                    _accessHasExpired = _accessTimeLeft <= TimeSpan.Zero;
                }
            }

            

            _isLoading = false;
            StateHasChanged();
            await gdsJs.InitGds(_cts.Token);
        }
    }
}

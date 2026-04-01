using FloodOnlineReportingTool.Database.Models.Eligibility;
using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.Public.Models.Order;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport.Overview;

public partial class Details(
    IFloodReportRepository floodReportRepository
) : IAsyncDisposable
{
    [CascadingParameter]
    public Task<AuthenticationState>? AuthenticationState { get; set; }

    [Parameter]
    public Guid? FloodReportId { get; set; }

    [Parameter]
    public string? FloodReportReference { get; set; }

    private PageInfo PreviousPage => FloodReportPages.Overview;

    private readonly CancellationTokenSource _cts = new();
    private bool _isLoading = true;
    private Database.Models.Flood.FloodReport? _floodReport;
    private bool _accessHasExpired = true;
    private TimeSpan _accessTimeLeft;

    // These are returning information about the current record
    private EligibilityOptions _floodInvestigation;
    //private IList<Organisation> _leadLocalFloodAuthorities = [];
    //private IList<Organisation> _otherFloodAuthorities = [];

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

    protected override async Task OnParametersSetAsync()
    {
        if (FloodReportId is not null)
        {
            _floodReport = await floodReportRepository.GetById((Guid)FloodReportId, _cts.Token);
        }
        else if (string.IsNullOrEmpty(FloodReportReference) == false)
        {
            _floodReport = await floodReportRepository.GetByReference((string)FloodReportReference, _cts.Token);
        }

        if (_floodReport is not null)
        {
            var result = await floodReportRepository.CalculateEligibilityWithReference(_floodReport.Reference, _cts.Token);

            //_leadLocalFloodAuthorities = [.. result.ResponsibleOrganisations.Where(o => o.FloodAuthorityId == FloodAuthorityIds.LeadLocalFloodAuthority)];
            //_otherFloodAuthorities = [.. result.ResponsibleOrganisations.Where(o => o.FloodAuthorityId != FloodAuthorityIds.LeadLocalFloodAuthority)];

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
    }

    private async Task<Database.Models.Flood.FloodReport?> GetUsersFloodReport()
    {
        string? userId = null;
        if (AuthenticationState is not null)
        {
            var authState = await AuthenticationState;
            userId = authState.User.Oid;
        }

        if (FloodReportId is not null)
        {
            if (userId is not null)
            {
                //return await floodReportRepository.GetById(userId, FloodReportId.Value, _cts.Token);
            }

            return _floodReport = await floodReportRepository.GetById(FloodReportId.Value, _cts.Token);
        }

        if (FloodReportReference is not null)
        {
            if (userId is not null)
            {
                //return await floodReportRepository.GetByReference(userId, FloodReportReference, _cts.Token);
            }

            return await floodReportRepository.GetByReference(FloodReportReference, _cts.Token);
        }

        throw new InvalidOperationException("Either FloodReportId or FloodReportReference must be provided.");
    }
}

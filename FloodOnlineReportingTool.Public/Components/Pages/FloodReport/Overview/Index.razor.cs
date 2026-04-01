using FloodOnlineReportingTool.Database.Models.Eligibility;
using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.Public.Models.Order;
using FloodOnlineReportingTool.Public.Services;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using NetTopologySuite.Index.HPRtree;
using System.Security.Claims;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport.Overview;

public partial class Index(
    IFloodReportRepository floodReportRepository,
    NavigationManager navigationManager,
    SessionStateService scopedSessionStorage
) : IPageOrder, IAsyncDisposable
{
    // Page order properties
    public string Title { get; set; } = FloodReportPages.Overview.Title;
    public IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [GeneralPages.Home.ToGdsBreadcrumb()];

    [CascadingParameter]
    public Task<AuthenticationState>? AuthenticationState { get; set; }

    private readonly CancellationTokenSource _cts = new();
    private bool _isLoading = true;
    private IList<Database.Models.Flood.FloodReport> _floodReport = [];
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
        string? userId = null;
        if (AuthenticationState is not null)
        {
            var authState = await AuthenticationState;
            userId = authState.User.Oid;
        }
        if (userId is null)
        {
            var floodReportId = await scopedSessionStorage.GetFloodReportId();
            var localReport = await floodReportRepository.GetById(floodReportId, _cts.Token);
            if (localReport is null)
            {
                return;
            }
            _floodReport.Add((Database.Models.Flood.FloodReport)localReport);
        }
        else
        {
            _floodReport = [.. await floodReportRepository.AllReportedByContact(userId, _cts.Token)];
        }

        if (_floodReport.Count > 0)
        {
            //var result = await floodReportRepository.CalculateEligibilityWithReference(_floodReport.Reference, _cts.Token);

            ////_leadLocalFloodAuthorities = [.. result.ResponsibleOrganisations.Where(o => o.FloodAuthorityId == FloodAuthorityIds.LeadLocalFloodAuthority)];
            ////_otherFloodAuthorities = [.. result.ResponsibleOrganisations.Where(o => o.FloodAuthorityId != FloodAuthorityIds.LeadLocalFloodAuthority)];

            //// These don't have any logic yet in the repository
            //_floodInvestigation = result.FloodInvestigation;
            //_isEmergencyResponse = result.IsEmergencyResponse;
            //_section19Url = result.Section19Url;
            //_section19 = result.Section19;
            //_propertyProtection = result.PropertyProtection;
            //_grantApplication = result.GrantApplication;

            //// Check if the users access has expired
            //if (_floodReport.ReportOwnerAccessUntil != null)
            //{
            //    _accessTimeLeft = _floodReport.ReportOwnerAccessUntil.Value - DateTimeOffset.UtcNow;
            //    _accessHasExpired = _accessTimeLeft <= TimeSpan.Zero;
            //}
        }

        _isLoading = false;
        StateHasChanged();
    }

    private void PerformAction(Guid FloodReportId)
    {
        navigationManager.NavigateTo($"{InvestigationPages.FirstPage.Url}/{FloodReportId}");
        StateHasChanged();
        return;
    }

    private void ViewReport(Guid FloodReportId)
    {
        navigationManager.NavigateTo($"{FloodReportPages.Overview.Url}/{FloodReportId}");
        StateHasChanged();
        return;
    }

    private void UpdateReport(Guid? EligibilityCheckId)
    {
        if (EligibilityCheckId.HasValue)
        {
            navigationManager.NavigateTo($"{FloodReportPages.Update.Url}/{EligibilityCheckId.Value}");
            StateHasChanged();
        }
        return;
    }
}

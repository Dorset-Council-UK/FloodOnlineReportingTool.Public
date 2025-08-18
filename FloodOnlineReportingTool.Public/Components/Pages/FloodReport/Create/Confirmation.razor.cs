using FloodOnlineReportingTool.Database.Models;
using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.Public.Models.Order;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport.Create;

public partial class Confirmation(
    ILogger<Confirmation> logger,
    IEligibilityCheckRepository eligibilityRepository,
    IGdsJsInterop gdsJs
) : IPageOrder, IAsyncDisposable
{
    // Page order properties
    public string Title { get; set; } = FloodReportCreatePages.Confirmation.Title;
    public IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [
        GeneralPages.Home.ToGdsBreadcrumb(),
        FloodReportPages.Home.ToGdsBreadcrumb(),
    ];

    [SupplyParameterFromQuery]
    private string? Reference { get; set; }

    private readonly CancellationTokenSource _cts = new();
    private bool _isLoading = true;
    private bool _loadingError;

    private bool _hasContactInformation;
    private EligibilityOptions _floodInvestigation;
    private IList<Organisation> _leadLocalFloodAuthorities = [];
    private IList<Organisation> _otherFloodAuthorities = [];

    // These don't have any logic yet in the repository
    private bool _isEmergencyResponse;
    private string? _section19Url;
    private EligibilityOptions _grantApplication;
    private EligibilityOptions _propertyProtection;
    private EligibilityOptions _section19;

    protected async override Task OnInitializedAsync()
    {
        if (!string.IsNullOrWhiteSpace(Reference))
        {
            try
            {
                var result = await eligibilityRepository.CalculateEligibilityWithReference(Reference, _cts.Token);

                _hasContactInformation = result.HasContactInformation;
                _floodInvestigation = result.FloodInvestigation;
                _leadLocalFloodAuthorities = [.. result.ResponsibleOrganisations.Where(o => o.FloodAuthorityId == FloodAuthorityIds.LeadLocalFloodAuthority)];
                _otherFloodAuthorities = [.. result.ResponsibleOrganisations.Where(o => o.FloodAuthorityId != FloodAuthorityIds.LeadLocalFloodAuthority)];

                // These don't have any logic yet in the repository
                _isEmergencyResponse = result.IsEmergencyResponse;
                _section19Url = result.Section19Url;
                _section19 = result.Section19;
                _propertyProtection = result.PropertyProtection;
                _grantApplication = result.GrantApplication;
            }
            catch (InvalidOperationException ex)
            {
                logger.LogError(ex, "There was a problem getting the eligibility check from the database");
                _loadingError = true;
            }
        }

        _isLoading = false;
    }

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
            await gdsJs.InitGds(_cts.Token);
        }
    }
}

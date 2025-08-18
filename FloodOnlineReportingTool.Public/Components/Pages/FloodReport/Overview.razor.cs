using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.Public.Models.Order;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport;

[Authorize]
public partial class Overview(
    IFloodReportRepository floodReportRepository,
    IGdsJsInterop gdsJs
) : IPageOrder, IAsyncDisposable
{
    // Page order properties
    public string Title { get; set; } = FloodReportPages.Overview.Title;
    public IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [ GeneralPages.Home.ToGdsBreadcrumb() ];

    [CascadingParameter]
    public Task<AuthenticationState>? AuthenticationState { get; set; }

    private readonly CancellationTokenSource _cts = new();
    private Guid _userId;
    private Database.Models.FloodReport? _floodReport;
    private bool _accessHasExpired = true;
    private TimeSpan _accessTimeLeft;

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
        // Setup model and edit context
        if (_floodReport == null)
        {
            if (AuthenticationState == null)
            {
                return;
            }

            // Get the user's ID and check if they have the admin role
            var authState = await AuthenticationState.ConfigureAwait(false);
            if (authState == null)
            {
                return;
            }

            _userId = authState.User.IdentityUserId() ?? Guid.Empty;
            _floodReport = await floodReportRepository.ReportedByUser(_userId, _cts.Token).ConfigureAwait(false);
            if (_floodReport != null)
            {
                // Check if the users access has expired
                if (_floodReport.UserAccessUntilUtc != null)
                {
                    _accessTimeLeft = _floodReport.UserAccessUntilUtc.Value - DateTimeOffset.UtcNow;
                    _accessHasExpired = _accessTimeLeft <= TimeSpan.Zero;
                }
            }
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await gdsJs.InitGds(_cts.Token);
        }
    }
}

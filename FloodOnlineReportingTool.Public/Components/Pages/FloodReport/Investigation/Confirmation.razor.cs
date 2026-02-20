using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.Public.Models.Order;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Identity.Web;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport.Investigation;

[Authorize]
public partial class Confirmation(
    IInvestigationRepository investigationRepository
) : IPageOrder, IAsyncDisposable
{
    // Page order properties
    public string Title { get; set; } = InvestigationPages.Confirmation.Title;
    public IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [
        GeneralPages.Home.ToGdsBreadcrumb(),
        FloodReportPages.Overview.ToGdsBreadcrumb(),
    ];

    [CascadingParameter]
    public Task<AuthenticationState>? AuthenticationState { get; set; }

    private Database.Models.Investigate.Investigation? _investigation;
    private readonly CancellationTokenSource _cts = new();

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
        var userId = await GetUserIdAsGuid();
        if (userId.HasValue)
        {
            _investigation = await investigationRepository.ReportedByUserBasicInformation(userId.Value, _cts.Token);
        }
    }

    private async Task<string?> GetUserId()
    {
        if (AuthenticationState is null)
        {
            return null;
        }
        var authState = await AuthenticationState;
        return authState.User.GetObjectId();
    }

    private async Task<Guid?> GetUserIdAsGuid()
    {
        return Guid.TryParse(await GetUserId(), out var userId) ? userId : null;
    }
}

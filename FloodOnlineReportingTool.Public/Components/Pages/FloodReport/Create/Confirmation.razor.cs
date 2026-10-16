using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.Public.Authentication;
using FloodOnlineReportingTool.Public.Models.Order;
using FloodOnlineReportingTool.Public.Services;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport.Create;

public partial class Confirmation(
    ILogger<Confirmation> logger,
    IEligibilityCheckRepository eligibilityRepository,
    IMediaItemRepository mediaItemRepository,
    IFloodReportSourceRepository floodReportSourceRepository,
    SessionStateService scopedSessionStorage,
    IAuthorizationService authorizationService,
    NavigationManager navigationManager
) : IPageOrder, IAsyncDisposable
{
    [SupplyParameterFromQuery]
    private string? Reference { get; set; }

    [CascadingParameter]
    public Task<AuthenticationState>? AuthenticationState { get; set; }

    // Private Fields
    private readonly CancellationTokenSource _cts = new();
    private bool _isLoading = true;
    private bool _loadingError;
    private bool _isAuthorised;
    private string? _userId;
    private Guid _FloodReportId;
    private bool _hasContactInformation;
    private IEnumerable<string?> _userIds = [];
    private int _mediaItemsAssociatedWithReport;

    // Public Properties
    public string Title { get; set; } = FloodReportCreatePages.Confirmation.Title;
    public IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [
        GeneralPages.Home.ToGdsBreadcrumb(),
        FloodReportPages.Home.ToGdsBreadcrumb(),
    ];

    private string RegisterLoginUrl => $"{AccountPages.SignIn.Url}?redirectUri={Uri.EscapeDataString(SubscriptionPages.Home.Url + "?Me=true&Owns=true")}";

    protected override async Task OnInitializedAsync()
    {
        // Check if user is authenticated
        if (AuthenticationState is not null)
        {
            var authState = await AuthenticationState;
            var user = authState.User;
            _userId = user.Oid;
        }
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
            // Suppressing exception during disposal to prevent issues during component teardown
        }

        GC.SuppressFinalize(this);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(Reference))
        {
            await LoadFromReferenceAsync();
        }
        else
        {
            await LoadFromSessionAsync();
        }

        if (!_loadingError && _FloodReportId != Guid.Empty)
        {
            var authState = AuthenticationState is not null
                ? await AuthenticationState
                : null;

            if (authState?.User.Identity?.IsAuthenticated == true)
            {
                _isAuthorised = await IsAuthenticatedOwnerOrAdminAsync();

                if (_isAuthorised)
                {
                    await scopedSessionStorage.SaveFloodReporSourceId(_FloodReportId);
                    await scopedSessionStorage.SaveConfirmationAccessConsumed(true);
                    await scopedSessionStorage.SaveConfirmationLoginPending(false);
                    await LoadMediaItemCountAsync();
                }
            }
            else
            {
                var sessionFloodReportId = await scopedSessionStorage.GetFloodReportSourceId();
                var confirmationAccessConsumed = await scopedSessionStorage.GetConfirmationAccessConsumed();
                var confirmationLoginPending = await scopedSessionStorage.GetConfirmationLoginPending();

                if (!_hasContactInformation && sessionFloodReportId == _FloodReportId && (!confirmationAccessConsumed || confirmationLoginPending))
                {
                    _isAuthorised = true;
                    await scopedSessionStorage.SaveConfirmationAccessConsumed(true);
                    await LoadMediaItemCountAsync();
                }
            }
        }

        _isLoading = false;
        StateHasChanged();
    }

    private async Task LoadFromReferenceAsync()
    {
        try
        {
            var result = await eligibilityRepository.GetByReference(Reference!, _cts.Token);
            var floodReportSource = result?.FloodReportSource;

            if (floodReportSource is null)
            {
                return;
            }

            _FloodReportId = floodReportSource.Id;
            _hasContactInformation = floodReportSource.ContactRecords.Count > 0;
            if (_hasContactInformation)
            {
                _userIds = floodReportSource.ContactRecords.Select(c => c.ContactUserId).ToArray();
            }
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "There was a problem getting the eligibility check from the database");
            _loadingError = true;
        }
    }

    private async Task<bool> IsAuthenticatedOwnerOrAdminAsync()
    {
        if (AuthenticationState is null)
        {
            return false;
        }

        var authState = await AuthenticationState;
        if (authState.User.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        var adminPolicyCheck = await authorizationService.AuthorizeAsync(authState.User, PolicyNames.Admin);
        if (adminPolicyCheck.Succeeded)
        {
            return true;
        }

        return _userId is not null && _userIds.Contains(_userId, StringComparer.OrdinalIgnoreCase);
    }

    private async Task OnRegisterLoginClickedAsync()
    {
        _isLoading = true;
        await scopedSessionStorage.SaveConfirmationLoginPending(true);
        await InvokeAsync(StateHasChanged);
        navigationManager.NavigateTo(RegisterLoginUrl, forceLoad: true);
    }

    private async Task LoadFromSessionAsync()
    {
        _FloodReportId = await scopedSessionStorage.GetFloodReportSourceId();

        if (_FloodReportId == Guid.Empty)
        {
            return;
        }

        var result = await floodReportSourceRepository.GetById(_FloodReportId, _cts.Token);
        if (result is null)
        {
            _FloodReportId = Guid.Empty;
            return;
        }

        Reference = result.Reference;
        _hasContactInformation = result.ContactRecords.Count > 0;
        if (_hasContactInformation)
        {
            _userIds = result.ContactRecords.Select(c => c.ContactUserId).ToArray();
        }
    }

    private async Task LoadMediaItemCountAsync()
    {
        _mediaItemsAssociatedWithReport = await mediaItemRepository.GetCountByReport(_FloodReportId, _cts.Token);
    }
}

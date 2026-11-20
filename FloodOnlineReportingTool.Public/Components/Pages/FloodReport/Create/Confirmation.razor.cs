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
    private bool _isAuthenticated = false;
    private bool _isReader = false;
    private bool _isSessionOwner = false;
    private bool _isContactOwner = false;
    private Guid _floodReportSourceId = Guid.Empty;
    private int _mediaItemsCount;

    // Public Properties
    public string Title { get; set; } = FloodReportCreatePages.Confirmation.Title;
    public IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [
        GeneralPages.Home.ToGdsBreadcrumb(),
        FloodReportPages.Home.ToGdsBreadcrumb(),
    ];

    private static string RegisterLoginUrl => $"{AccountPages.SignIn.Url}?redirectUri={Uri.EscapeDataString(SubscriptionPages.Home.Url + "?Me=true&Owns=true")}";

    protected override async Task OnInitializedAsync()
    {
        if (string.IsNullOrWhiteSpace(Reference))
        {
            logger.LogError("Reference is missing in the query string for the confirmation page");
        }

        // check permissions

        if (AuthenticationState is not null)
        {
            var authState = await AuthenticationState;
            _isAuthenticated = authState.User.Identity?.IsAuthenticated == true;
            if (_isAuthenticated)
            {
                var readerPolicyCheck = await authorizationService.AuthorizeAsync(authState.User, PolicyNames.Reader);
                _isReader = readerPolicyCheck.Succeeded;
                logger.LogInformation("User is signed in, reader policy: {IsReader}", _isReader);

                string? userId = authState.User.Oid;
                if (!string.IsNullOrWhiteSpace(userId) && !string.IsNullOrWhiteSpace(Reference))
                {
                    // Check if the user has reported a flood report source
                    var eligibilityCheck = await eligibilityRepository.ReportedByUser(userId, Reference, _cts.Token);
                    if (eligibilityCheck is null)
                    {
                        _isContactOwner = false;
                        _isSessionOwner = false;
                        _floodReportSourceId = Guid.Empty;
                    }
                    else
                    {
                        _isContactOwner = true;
                        _isSessionOwner = false;
                        _floodReportSourceId = eligibilityCheck.FloodReportSource?.Id ?? Guid.Empty;
                    }

                    logger.LogInformation("Signed in user, contact owner: {IsContactOwner}, flood report source ID: {FloodReportSourceId}", _isContactOwner, _floodReportSourceId);
                }
            }
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
        if (firstRender)
        {
            // read from protected session storage

            if (_isAuthenticated && _isReader)
            {
                // signed in and reader
                await scopedSessionStorage.SaveFloodReportSourceId(_floodReportSourceId);
                await scopedSessionStorage.SaveConfirmationAccessConsumed(consumed: true);
                await scopedSessionStorage.SaveConfirmationLoginPending(pending: false);
            }

            if (!_isAuthenticated)
            {
                // not signed in
                logger.LogInformation("User is not signed in");

                if (!string.IsNullOrWhiteSpace(Reference))
                {
                    // need the reference
                    var eligibilityCheck = await eligibilityRepository.GetByReference(Reference, _cts.Token);
                    _floodReportSourceId = eligibilityCheck?.FloodReportSource?.Id ?? Guid.Empty;

                    var sessionFloodReportSourceId = await scopedSessionStorage.GetFloodReportSourceId();
                    var sessionConfirmationAccessConsumed = await scopedSessionStorage.GetConfirmationAccessConsumed();
                    var sessionConfirmationLoginPending = await scopedSessionStorage.GetConfirmationLoginPending();

                    _isContactOwner = false;
                    _isSessionOwner = sessionFloodReportSourceId != Guid.Empty && sessionFloodReportSourceId == _floodReportSourceId;
                    logger.LogInformation("Not signed in user, session owner: {IsSessionOwner}, flood report source ID: {FloodReportSourceId}", _isSessionOwner, _floodReportSourceId);

                    if (_isSessionOwner && (!sessionConfirmationAccessConsumed || sessionConfirmationLoginPending))
                    {
                        await scopedSessionStorage.SaveConfirmationAccessConsumed(consumed: true);
                    }
                }
            }

            _mediaItemsCount = await LoadMediaItemCountAsync();

            _isLoading = false;
            StateHasChanged();

            bool hasAccess = (_isAuthenticated && _isReader) || _isContactOwner || _isSessionOwner;
            if (!hasAccess)
            {
                logger.LogWarning("Access denied to confirmation page for reference {Reference}. Session owner and contact owner could not be worked out.", Reference);
                navigationManager.NavigateTo(GeneralPages.AccessDenied.Url, forceLoad: true);
                return;
            }
        }
    }

    private async Task OnRegisterLoginClickedAsync()
    {
        await scopedSessionStorage.SaveConfirmationLoginPending(pending: true);
        await InvokeAsync(StateHasChanged);
        navigationManager.NavigateTo(RegisterLoginUrl, forceLoad: true);
    }

    private async Task<int> LoadMediaItemCountAsync()
    {
        if (_floodReportSourceId == Guid.Empty)
        {
            return 0;
        }
        return await mediaItemRepository.GetCountByReport(_floodReportSourceId, _cts.Token);
    }
}

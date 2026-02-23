using FloodOnlineReportingTool.Database.Models.Eligibility;
using FloodOnlineReportingTool.Database.Models.Flood;
using FloodOnlineReportingTool.Database.Models.Investigate;
using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.Public.Models;
using FloodOnlineReportingTool.Public.Models.Order;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Security.Claims;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport.Investigation;

[Authorize]
public partial class Summary(
    ILogger<Summary> logger,
    IEligibilityCheckRepository eligibilityCheckRepository,
    IInvestigationRepository investigationRepository,
    ProtectedSessionStorage protectedSessionStorage,
    NavigationManager navigationManager
) : IAsyncDisposable
{
    [CascadingParameter]
    public Task<AuthenticationState>? AuthenticationState { get; set; }

    private InvestigationDto? _investigationDto;
    private bool _investigationIsComplete;
    private readonly CancellationTokenSource _cts = new();
    private bool _isLoading = true;
    private bool _isInternal;
    private ICollection<string> _saveErrors = [];

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

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            if (AuthenticationState is not null)
            {
                var authState = await AuthenticationState;
                var userId = authState.User.Oid;
                if (userId is not null)
                {
                    var eligibilityCheck = await eligibilityCheckRepository.ReportedByUser(userId, _cts.Token);
                    _isInternal = eligibilityCheck?.IsInternal() == true;
                }
            }
            else
            {
                _isInternal = false;
            }

            _investigationDto = await GetInvestigation();
            _investigationIsComplete = _investigationDto.IsComplete(_isInternal);

            if (!_investigationIsComplete)
            {
                logger.LogWarning("Investigation information is not complete. Investigation: {Investigation}", _investigationDto);
                _saveErrors.Add("Your investigation information is not complete. Please check the information below and complete any missing information.");
            }

            _isLoading = false;
            StateHasChanged();
        }
    }

    private async Task SaveInvestigation()
    {
        _saveErrors.Clear();

        if (_investigationDto is null)
        {
            logger.LogError("Investigation information was not found. Investigation: {Investigation}", _investigationDto);
            _saveErrors.Add("Not able to find the investigation information.");
        }

        string? userId = null;
        if (AuthenticationState is not null)
        {
            var authState = await AuthenticationState;
            userId = authState.User.Oid;
        }
        if (userId is null)
        {
            logger.LogError("User ID was not found.");
            _saveErrors.Add("Not able to identify you.");
        }

        if (_saveErrors.Count > 0 || _investigationDto is null || userId is null)
        {
            _saveErrors.Add("There was a problem saving the investigation. Please try again but if this issue happens again then please report a bug.");
            return;
        }

        logger.LogDebug("Saving investigation information..");
        try
        {
            await investigationRepository.CreateForUser(userId, _investigationDto, _cts.Token);

            // Clear the stored data
            await protectedSessionStorage.DeleteAsync(SessionConstants.Investigation);

            logger.LogInformation("Investigation created successfully for user");
            navigationManager.NavigateTo(InvestigationPages.Confirmation.Url);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "There was a problem creating the investigation information");
            _saveErrors.Add("There was a problem saving the investigation. Please try again but if this issue happens again then please report a bug.");
        }
    }

    private async Task<InvestigationDto> GetInvestigation()
    {
        var data = await protectedSessionStorage.GetAsync<InvestigationDto>(SessionConstants.Investigation);
        if (data.Success && data.Value is not null)
        {
            return data.Value;
        }

        logger.LogWarning("Investigation was not found in the protected storage.");
        return new InvestigationDto();
    }
}
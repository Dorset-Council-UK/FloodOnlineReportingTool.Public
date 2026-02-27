using FloodOnlineReportingTool.Database.Models.Eligibility;
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
    private readonly ICollection<string> _summaryErrors = [];
    private readonly CancellationTokenSource _cts = new();
    private bool _isLoading = true;
    private bool _isInternal;

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
            _isInternal = await GetIsInternalFloodImpacts();
            _investigationDto = await GetInvestigation();

            _isLoading = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Get one of the users eligibility checks and read if it has any internal flood impacts.
    /// </summary>
    private async Task<bool> GetIsInternalFloodImpacts()
    {
        if (AuthenticationState is not null)
        {
            var authState = await AuthenticationState;
            var userId = authState.User.Oid;
            if (userId is not null)
            {
                var eligibilityCheck = await eligibilityCheckRepository.ReportedByUser(userId, _cts.Token);
                return eligibilityCheck?.IsInternal() == true;
            }
        }

        return false;
    }

    private void OnValidationStatusChanged(bool isValid)
    {
        if (isValid)
        {
            _summaryErrors.Clear();
            return;
        }

        logger.LogWarning("Investigation summary is not valid.");
        const string generalMessage = "Your investigation information is not complete. Please check the information below and complete any missing information.";
        if (!_summaryErrors.Contains(generalMessage, StringComparer.OrdinalIgnoreCase))
        {
            _summaryErrors.Add(generalMessage);
        }
    }

    private async Task OnAcceptAndSend()
    {
        _summaryErrors.Clear();

        if (true || _investigationDto is null)
        {
            logger.LogError("Investigation information was not found");
            _summaryErrors.Add("Not able to find the investigation information");
            return;
        }

        string? userId = null;
        if (AuthenticationState is not null)
        {
            var authState = await AuthenticationState;
            userId = authState.User.Oid;
        }
        if (true || userId is null)
        {
            logger.LogError("User ID was not found.");
            _summaryErrors.Add("Not able to identify you");
            return;
        }

        await SaveInvestigation(_investigationDto, userId);
    }

    private async Task SaveInvestigation(InvestigationDto dto, string userId)
    {
        logger.LogDebug("Saving investigation information..");
        try
        {
            await investigationRepository.CreateForUser(userId, dto, _cts.Token);

            // Clear the stored data
            await protectedSessionStorage.DeleteAsync(SessionConstants.Investigation);

            logger.LogInformation("Investigation created successfully for user");
            navigationManager.NavigateTo(InvestigationPages.Confirmation.Url);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "There was a problem creating the investigation information");
            _summaryErrors.Add("There was a problem saving the investigation. Please try again but if this issue happens again then please report a bug.");
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
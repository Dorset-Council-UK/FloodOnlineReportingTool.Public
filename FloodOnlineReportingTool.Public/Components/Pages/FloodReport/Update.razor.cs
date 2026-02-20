using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.Public.Models.FloodReport.Update;
using FloodOnlineReportingTool.Public.Models.Order;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Identity.Web;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport;

[Authorize]
public partial class Update(
    ILogger<Update> logger,
    NavigationManager navigationManager,
    IEligibilityCheckRepository eligibilityCheckRepository
) : IPageOrder, IAsyncDisposable
{
    // Page order properties
    public string Title { get; set; } = FloodReportPages.Update.Title;
    public IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [
        GeneralPages.Home.ToGdsBreadcrumb(),
        FloodReportPages.Overview.ToGdsBreadcrumb(),
    ];

    [Parameter]
    public Guid EligibilityCheckId { get; set; }

    [CascadingParameter]
    public Task<AuthenticationState>? AuthenticationState { get; set; }

    private UpdateModel? _updateModel;
    private EditContext _editContext = default!;
    private ValidationMessageStore _messageStore = default!;
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
        // Setup model and edit context
        if (_updateModel == null)
        {
            _updateModel = await GetUpdateModel();

            if (_updateModel != null)
            {
                _editContext = new(_updateModel);
                _editContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
                _messageStore = new(_editContext);
            }
        }
    }

    private async Task OnSubmit()
    {
        _messageStore.Clear();

        if (!_editContext.Validate())
        {
            return;
        }

        await UpdateFloodReport();
    }

    private async Task UpdateFloodReport()
    {
        if (_updateModel is null)
        {
            logger.LogError("Update model was null. Cannot update flood report.");
        }

        var userId = await GetUserIdAsGuid();
        if (userId is null)
        {
            logger.LogError("User ID was not found.");
        }

        if (_updateModel is null || userId is null)
        {
            _messageStore.Add(_editContext.Field(nameof(_updateModel.UprnText)), "There was a problem updating the flood report. Please try again but if this issue happens again then please report a bug.");
            _editContext.NotifyValidationStateChanged();
            return;
        }

        logger.LogDebug("Updating flood report");
        try
        {
            await eligibilityCheckRepository.UpdateForUser(userId.Value, EligibilityCheckId, _updateModel.ToDto(), _cts.Token);
            logger.LogInformation("Eligibility check updated successfully for user {UserId}", userId.Value);
            navigationManager.NavigateTo(FloodReportPages.Overview.Url);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "There was a problem updating the eligibility check");
            _messageStore.Add(_editContext.Field(nameof(_updateModel.UprnText)), $"There was a problem updating the flood report. Please try again but if this issue happens again then please report a bug.");
            _editContext.NotifyValidationStateChanged();
        }
    }

    private async Task<UpdateModel?> GetUpdateModel()
    {
        var userId = await GetUserIdAsGuid();
        if (userId.HasValue)
        {
            var eligibilityCheck = await eligibilityCheckRepository.ReportedByUser(userId.Value, EligibilityCheckId, _cts.Token);
            if (eligibilityCheck is not null)
            {
                return eligibilityCheck.ToUpdateModel();
            }
        }

        return null;
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

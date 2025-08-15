using FloodOnlineReportingTool.Database.Models;
using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.Public.Models.FloodReport.Update;
using FloodOnlineReportingTool.Public.Models.Order;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport;

[Authorize]
public partial class Update(
    ILogger<Update> logger,
    NavigationManager navigationManager,
    IEligibilityCheckRepository eligibilityCheckRepository,
    IGdsJsInterop gdsJs
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
    private Guid _userId;

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
            _userId = await AuthenticationState.IdentityUserId() ?? Guid.Empty;
            _updateModel = await GetUpdateModel();

            if (_updateModel != null)
            {
                _editContext = new(_updateModel);
                _editContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
                _messageStore = new(_editContext);
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
        logger.LogDebug("Updating flood report");

        if (_updateModel == null)
        {
            return;
        }

        try
        {
            var dto = _updateModel.ToDto();
            await eligibilityCheckRepository.UpdateForUser(_userId, EligibilityCheckId, dto, _cts.Token);
            logger.LogInformation("Eligibility check updated successfully for user {UserId}", _userId);
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
        var eligibilityCheck = await eligibilityCheckRepository.ReportedByUser(_userId, EligibilityCheckId, _cts.Token);
        if (eligibilityCheck == null)
        {
            return null;
        }
        return eligibilityCheck.ToUpdateModel();
    }
}

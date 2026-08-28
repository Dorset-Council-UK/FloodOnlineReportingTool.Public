using FloodOnlineReportingTool.Database.Models.Flood;
using FloodOnlineReportingTool.Database.Models.Investigate;
using FloodOnlineReportingTool.Database.Models.Status;
using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.Public.Models;
using FloodOnlineReportingTool.Public.Models.Order;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport.Investigation;

[Authorize]
public partial class History(
    ILogger<History> logger,
    ICommonRepository commonRepository,
    ProtectedSessionStorage protectedSessionStorage,
    NavigationManager navigationManager
) : IAsyncDisposable
{
    // Page order properties
    public string Title { get; set; } = InvestigationPages.History.Title;

    private Models.FloodReport.Investigation.History Model { get; set; } = default!;

    private PageInfo NextPage => InvestigationPages.Summary;
    private PageInfo? PreviousPage;

    private EditContext _editContext = default!;
    private readonly CancellationTokenSource _cts = new();
    private bool _isLoading = true;
    private IList<RecordStatus> _historyOfFloodingOptions = [];
    private IList<RecordStatus> _propertyInsuredOptions = [];

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
        if (Model is null)
        {
            // Setup model and edit context
            Model ??= new();
            _editContext = new(Model);
            _editContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
        }

        _historyOfFloodingOptions = await commonRepository.GetRecordStatusesByCategory(RecordStatusCategory.General, _cts.Token);
        _propertyInsuredOptions = await commonRepository.GetRecordStatusesByCategory(RecordStatusCategory.General, _cts.Token);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Set any previously entered data
            var investigation = await GetInvestigation();

            PreviousPage = investigation.WarningSources.Contains(FloodMitigationIds.FloodlineWarning) 
                ? InvestigationPages.Floodline 
                : InvestigationPages.WarningSources;

            Model.HistoryOfFloodingId = investigation.HistoryOfFloodingId;
            Model.HistoryOfFloodingDetails = investigation.HistoryOfFloodingDetails;
            Model.PropertyInsuredId = investigation.PropertyInsuredId;

            _isLoading = false;
            StateHasChanged();
        }
    }

    private async Task OnValidSubmit()
    {
        var investigation = await GetInvestigation();
        var updatedInvestigation = investigation with
        {
            HistoryOfFloodingId = Model.HistoryOfFloodingId,
            HistoryOfFloodingDetails = Model.HistoryOfFloodingId == RecordStatusIds.Yes ? Model.HistoryOfFloodingDetails : null,
            PropertyInsuredId = Model.PropertyInsuredId,
        };
        await protectedSessionStorage.SetAsync(SessionConstants.Investigation, updatedInvestigation);

        // Go to the summary
        navigationManager.NavigateTo(NextPage.Url);
    }

    private async Task<InvestigationDto> GetInvestigation()
    {
        var data = await protectedSessionStorage.GetAsync<InvestigationDto>(SessionConstants.Investigation);
        if (data.Success)
        {
            if (data.Value != null)
            {
                return data.Value;
            }
        }

        logger.LogWarning("Investigation was not found in the protected storage.");
        return new InvestigationDto();
    }

}
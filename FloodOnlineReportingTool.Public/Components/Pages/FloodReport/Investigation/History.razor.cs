using FloodOnlineReportingTool.DataAccess.Models;
using FloodOnlineReportingTool.DataAccess.Repositories;
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
    NavigationManager navigationManager,
    IGdsJsInterop gdsJs
) : IPageOrder, IAsyncDisposable
{
    // Page order properties
    public string Title { get; set; } = InvestigationPages.History.Title;
    public IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [
        GeneralPages.Home.ToGdsBreadcrumb(),
        FloodReportPages.Overview.ToGdsBreadcrumb(),
    ];

    private Models.FloodReport.Investigation.History Model { get; set; } = default!;

    private EditContext _editContext = default!;
    private readonly CancellationTokenSource _cts = new();
    private bool _isLoading = true;
    private IReadOnlyCollection<GdsOptionItem<Guid>> _historyOfFloodingOptions = [];

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

    protected override void OnInitialized()
    {
        // Setup model and edit context
        Model ??= new();
        _editContext = new(Model);
        _editContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Set any previously entered data
            var investigation = await GetInvestigation();

            var pageInfo = investigation.WarningSources.Contains(FloodMitigationIds.FloodlineWarning) ? InvestigationPages.Floodline : InvestigationPages.WarningSources;
            Breadcrumbs = Breadcrumbs.Append(pageInfo.ToGdsBreadcrumb()).ToList();

            Model.HistoryOfFloodingId = investigation.HistoryOfFloodingId;
            Model.HistoryOfFloodingDetails = investigation.HistoryOfFloodingDetails;
            _historyOfFloodingOptions = await CreateHistoryOfFloodingOptions(investigation.HistoryOfFloodingId);

            _isLoading = false;
            StateHasChanged();

            await gdsJs.InitGds(_cts.Token);
        }
    }

    private async Task OnValidSubmit()
    {
        var investigation = await GetInvestigation();
        var updatedInvestigation = investigation with
        {
            HistoryOfFloodingId = Model.HistoryOfFloodingId,
            HistoryOfFloodingDetails = Model.HistoryOfFloodingId == RecordStatusIds.Yes ? Model.HistoryOfFloodingDetails : null,
        };
        await protectedSessionStorage.SetAsync(SessionConstants.Investigation, updatedInvestigation);

        // Go to the summary
        navigationManager.NavigateTo(InvestigationPages.Summary.Url);
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

    private async Task<IReadOnlyCollection<GdsOptionItem<Guid>>> CreateHistoryOfFloodingOptions(Guid? selectedValue)
    {
        const string idPrefix = "history-of-flooding";
        var recordStatuses = await commonRepository.GetRecordStatusesByCategory(RecordStatusCategory.General, _cts.Token);
        return [.. recordStatuses.Select(o => CreateOption(o, idPrefix, selectedValue))];
    }

    private static GdsOptionItem<Guid> CreateOption(RecordStatus recordStatus, string idPrefix, Guid? selectedValue)
    {
        var id = $"{idPrefix}-{recordStatus.Id}".AsSpan();
        var label = recordStatus.Text.AsSpan();
        var selected = recordStatus.Id == selectedValue;
        var isExclusive = recordStatus.Id == RecordStatusIds.NotSure;

        return new GdsOptionItem<Guid>(id, label, recordStatus.Id, selected, isExclusive);
    }
}
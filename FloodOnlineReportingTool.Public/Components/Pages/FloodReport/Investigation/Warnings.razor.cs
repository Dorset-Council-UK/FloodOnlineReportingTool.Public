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
public partial class Warnings(
    ILogger<Warnings> logger,
    ICommonRepository commonRepository,
    ProtectedSessionStorage protectedSessionStorage,
    NavigationManager navigationManager
) : IAsyncDisposable
{
    // Page order properties
    public string Title { get; set; } = InvestigationPages.Warnings.Title;

    [SupplyParameterFromQuery]
    private bool FromSummary { get; set; }
    private PageInfo NextPage => FromSummary
        ? InvestigationPages.Summary
        : InvestigationPages.WarningSources;
    private static PageInfo PreviousPage => InvestigationPages.HelpReceived;

    private Models.FloodReport.Investigation.Warnings Model { get; set; } = default!;

    private EditContext _editContext = default!;
    private readonly CancellationTokenSource _cts = new();
    private bool _isLoading = true;
    private IList<RecordStatus> _registeredWithFloodlineOptions = [];
    private IList<RecordStatus> _otherWarningOptions = [];

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
        var recordStatuses = await GetRecordStatusesWithoutNotSure();
        _registeredWithFloodlineOptions = recordStatuses.ToList();
        _otherWarningOptions = recordStatuses.ToList();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Set any previously entered data
            var investigation = await GetInvestigation();
            Model.OtherWarningId = investigation.WarningReceivedId;
            Model.RegisteredWithFloodlineId = investigation.FloodlineId;

            _isLoading = false;
            StateHasChanged();  
        }
    }

    private async Task OnValidSubmit()
    {
        var investigation = await GetInvestigation();
        var updatedInvestigation = investigation with
        {
            FloodlineId = Model.RegisteredWithFloodlineId,
            WarningReceivedId = Model.OtherWarningId,
        };
        await protectedSessionStorage.SetAsync(SessionConstants.Investigation, updatedInvestigation);

        // Go to the next page or back to the summary
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

    private async Task<IReadOnlyCollection<RecordStatus>> GetRecordStatusesWithoutNotSure()
    {
        var recordStatuses = await commonRepository.GetRecordStatusesByCategory(RecordStatusCategory.General, _cts.Token);
        return recordStatuses.Where(o => o.Id != RecordStatusIds.NotSure).ToList();
    }

}

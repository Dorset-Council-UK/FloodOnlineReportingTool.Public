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
public partial class PeakDepthsPage(
    ILogger<PeakDepthsPage> logger,
    ICommonRepository commonRepository,
    ProtectedSessionStorage protectedSessionStorage,
    NavigationManager navigationManager
) : IAsyncDisposable
{
    private List<Models.FloodReport.Investigation.PeakDepth> Model { get; set; } = default!;

    public string Title { get; set; } = InvestigationPages.PeakDepthsPage.Title;

    private bool FromSummary { get; set; }
    private PageInfo NextPage => FromSummary
        ? InvestigationPages.Summary
        : InvestigationPages.ServiceImpact;
    private PageInfo? PreviousPage;

    private EditContext _editContext = default!;
    private readonly CancellationTokenSource _cts = new();
    private bool _isLoading = true;

    private Models.FloodReport.Investigation.PeakDepth PeakDepthModel = new();

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

            Model = investigation.InvestigationPeakDepths?
                .Select(peakDepth => new Models.FloodReport.Investigation.PeakDepth
                {
                    IsPeakDepthKnownId = peakDepth.IsPeakDepthKnownId,
                    InsideCentimetresText = peakDepth.PeakInsideCentimetres?.ToString(),
                    OutsideCentimetresText = peakDepth.PeakOutsideCentimetres?.ToString(),
                })
                .ToList()
                ?? new List<Models.FloodReport.Investigation.PeakDepth>();

            logger.LogInformation(
                "InvestigationPeakDepths: {SourceCount}, Model: {ModelCount}",
                investigation.InvestigationPeakDepths?.Count ?? 0,
                Model.Count);

            _isLoading = false;
            StateHasChanged();
        }
    }

    private Task AddPeakDepth(Models.FloodReport.Investigation.PeakDepth peakDepth)
    {
        Model.Add(peakDepth);
        return Task.CompletedTask;
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
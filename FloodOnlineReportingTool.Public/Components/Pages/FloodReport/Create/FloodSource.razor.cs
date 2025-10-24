using FloodOnlineReportingTool.Contracts.Shared;
using FloodOnlineReportingTool.Database.Models.Eligibility;
using FloodOnlineReportingTool.Database.Models.Flood;
using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.Public.Models;
using FloodOnlineReportingTool.Public.Models.Order;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport.Create;

public partial class FloodSource(
    ILogger<FloodSource> logger,
    ICommonRepository commonRepository,
    ProtectedSessionStorage protectedSessionStorage,
    NavigationManager navigationManager,
    IGdsJsInterop gdsJs
) : IPageOrder, IAsyncDisposable
{
    // Page order properties
    public string Title { get; set; } = FloodReportCreatePages.FloodSource.Title;
    public IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [
        GeneralPages.Home.ToGdsBreadcrumb(),
        FloodReportPages.Home.ToGdsBreadcrumb(),
    ];

    private Models.FloodReport.Create.FloodSource Model { get; set; } = default!;

    private EditContext _editContext = default!;
    private readonly CancellationTokenSource _cts = new();
    private bool _isLoading = true;

    protected override void OnInitialized()
    {
        // Setup model and edit context
        Model ??= new();
        _editContext = new(Model);
        _editContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
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
        }

        GC.SuppressFinalize(this);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var eligibilityCheck = await GetEligibilityCheck();

            var previousCrumb = eligibilityCheck.OnGoing ? FloodReportCreatePages.FloodStarted : FloodReportCreatePages.FloodDuration;
            Breadcrumbs = Breadcrumbs.Append(previousCrumb.ToGdsBreadcrumb()).ToList();

            Model.FloodSourceOptions = await CreateFloodSourceOptions(eligibilityCheck.Sources);

            _isLoading = false;
            StateHasChanged();

            await gdsJs.InitGds(_cts.Token);
        }
    }

    private async Task OnValidSubmit()
    {
        // Update the eligibility check
        var eligibilityCheck = await GetEligibilityCheck();

        var selectedOptions = Model.FloodSourceOptions.Where(o => o.Selected).Select(o => o.Value);
        IList<Guid> secondarySources = eligibilityCheck.SecondarySources;
        if (!selectedOptions.Contains(PrimaryCauseIds.RainwaterFlowingOverTheGround))
        {
            // We need to remove any run off options as it has not been selected
            secondarySources = [];
        }
        var updated = eligibilityCheck with
        {
            Sources = [.. Model.FloodSourceOptions.Where(o => o.Selected).Select(o => o.Value)],
            SecondarySources = secondarySources
        };

        await protectedSessionStorage.SetAsync(SessionConstants.EligibilityCheck, updated);

        if (updated.Sources.Contains(PrimaryCauseIds.RainwaterFlowingOverTheGround))
        {
            // We need to know more if they have selected this option
            navigationManager.NavigateTo(FloodReportCreatePages.FloodSecondarySource.Url);
        }
        else
        {
            // Go to the next page, which is always the summary
            navigationManager.NavigateTo(FloodReportCreatePages.Summary.Url);
        }
    }

    private async Task<EligibilityCheckDto> GetEligibilityCheck()
    {
        var data = await protectedSessionStorage.GetAsync<EligibilityCheckDto>(SessionConstants.EligibilityCheck);
        if (data.Success)
        {
            if (data.Value != null)
            {
                return data.Value;
            }
        }

        logger.LogDebug("Eligibility Check was not found in the protected storage.");
        return new();
    }

    private async Task<IReadOnlyCollection<GdsOptionItem<Guid>>> CreateFloodSourceOptions(IList<Guid> selectedValues)
    {
        const string idPrefix = "flood-source";
        var floodProblems = await commonRepository.GetFloodProblemsByCategory(FloodProblemCategory.PrimaryCause, _cts.Token);
        return [.. floodProblems.Select((o, idx) => CreateOption(o, idPrefix, selectedValues))];
    }

    private static GdsOptionItem<Guid> CreateOption(FloodProblem floodProblem, string idPrefix, IList<Guid> selectedValues)
    {
        var id = $"{idPrefix}-{floodProblem.Id}".AsSpan();
        var label = floodProblem.TypeName.AsSpan();
        var selected = selectedValues.Contains(floodProblem.Id);
        var isExclusive = floodProblem.Id == PrimaryCauseIds.NotSure;

        return new GdsOptionItem<Guid>(id, label, floodProblem.Id, selected, isExclusive);
    }
}


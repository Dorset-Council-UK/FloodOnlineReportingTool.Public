using FloodOnlineReportingTool.Database.Models.Flood;
using FloodOnlineReportingTool.Database.Models.Investigate;
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
public partial class Speed(
    ILogger<Speed> logger,
    ICommonRepository commonRepository,
    ProtectedSessionStorage protectedSessionStorage,
    NavigationManager navigationManager,
    IGdsJsInterop gdsJs
) : IPageOrder, IAsyncDisposable
{
    // Page order properties
    public string Title { get; set; } = InvestigationPages.Speed.Title;
    public IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [
        GeneralPages.Home.ToGdsBreadcrumb(),
        FloodReportPages.Overview.ToGdsBreadcrumb(),
        InvestigationPages.Home.ToGdsBreadcrumb(),
    ];

    [SupplyParameterFromQuery]
    private bool FromSummary { get; set; }

    private Models.FloodReport.Investigation.Speed Model { get; set; } = default!;

    private EditContext _editContext = default!;
    private readonly CancellationTokenSource _cts = new();
    private bool _isLoading = true;
    private IReadOnlyCollection<GdsOptionItem<Guid>> _beginOptions = [];
    private IReadOnlyCollection<GdsOptionItem<Guid>> _waterSpeedOptions = [];
    private IReadOnlyCollection<GdsOptionItem<Guid>> _appearanceOptions = [];

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
            Model.Begin = investigation.BeginId;
            Model.WaterSpeed = investigation.WaterSpeedId;
            Model.Appearance = investigation.AppearanceId;
            Model.MoreDetails = investigation.MoreAppearanceDetails;

            _beginOptions = await CreateBeginOptions();
            _waterSpeedOptions = await CreateWaterSpeedOptions();
            _appearanceOptions = await CreateAppearanceOptions();

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
            BeginId = Model.Begin,
            WaterSpeedId = Model.WaterSpeed,
            AppearanceId = Model.Appearance,
            MoreAppearanceDetails = Model.MoreDetails,
        };
        await protectedSessionStorage.SetAsync(SessionConstants.Investigation, updatedInvestigation);

        // Go to the next page or back to the summary
        var nextPage = FromSummary ? InvestigationPages.Summary : InvestigationPages.Destination;
        navigationManager.NavigateTo(nextPage.Url);
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

    private async Task<IReadOnlyCollection<GdsOptionItem<Guid>>> CreateBeginOptions()
    {
        var floodProblems = await commonRepository.GetFloodProblemsByCategory(FloodProblemCategory.WaterOnset, _cts.Token);
        return [.. floodProblems.Select(o => CreateOption(o, "begin", Model.Begin))];
    }

    private async Task<IReadOnlyCollection<GdsOptionItem<Guid>>> CreateWaterSpeedOptions()
    {
        var floodProblems = await commonRepository.GetFloodProblemsByCategory(FloodProblemCategory.Speed, _cts.Token);
        return [.. floodProblems.Select(o => CreateOption(o, "speed", Model.WaterSpeed))];
    }

    private async Task<IReadOnlyCollection<GdsOptionItem<Guid>>> CreateAppearanceOptions()
    {
        var floodProblems = await commonRepository.GetFloodProblemsByCategory(FloodProblemCategory.Appearance, _cts.Token);
        return [.. floodProblems.Select(o => CreateOption(o, "appearance", Model.Appearance))];
    }

    private static GdsOptionItem<Guid> CreateOption(FloodProblem floodProblem, string idPrefix, Guid? selectedValue)
    {
        var id = $"{idPrefix}-{floodProblem.Id}".AsSpan();
        var label = floodProblem.TypeName.AsSpan();
        var selected = floodProblem.Id == selectedValue;

        return new GdsOptionItem<Guid>(id, label, floodProblem.Id, selected);
    }
}

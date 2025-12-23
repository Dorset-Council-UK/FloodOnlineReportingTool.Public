using FloodOnlineReportingTool.Database.Models.Eligibility;
using FloodOnlineReportingTool.Database.Models.Flood;
using FloodOnlineReportingTool.Database.Models.Flood.FloodProblemIds;
using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.Public.Models;
using FloodOnlineReportingTool.Public.Models.Order;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Globalization;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport.Create;

public partial class FloodDuration(
    ILogger<FloodDuration> logger,
    ICommonRepository commonRepository,
    ProtectedSessionStorage protectedSessionStorage,
    NavigationManager navigationManager
) : IPageOrder, IAsyncDisposable
{
    // Page order properties
    public string Title { get; set; } = FloodReportCreatePages.FloodDuration.Title;
    public IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [
        GeneralPages.Home.ToGdsBreadcrumb(),
        FloodReportPages.Home.ToGdsBreadcrumb(),
        FloodReportCreatePages.FloodStarted.ToGdsBreadcrumb(),
    ];

    [SupplyParameterFromQuery]
    private bool FromSummary { get; set; }

    private Models.FloodReport.Create.FloodDuration Model { get; set; } = default!;

    private EditContext _editContext = default!;
    private readonly CancellationTokenSource _cts = new();
    private bool _isLoading = true;
    private DateTimeOffset? _floodStart;
    private bool _isFloodOngoing; // should be false
    private IReadOnlyCollection<GdsOptionItem<Guid>> _durationOptions { get; set; } = [];

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
            var eligibilityCheck = await GetEligibilityCheck();

            _floodStart = eligibilityCheck.ImpactStart;
            _isFloodOngoing = eligibilityCheck.OnGoing;

            Model.DurationKnownId = eligibilityCheck.DurationKnownId;
            if (Model.DurationKnownId == FloodDurationIds.DurationKnown)
            {
                if (eligibilityCheck.ImpactDuration != null)
                {
                    var days = eligibilityCheck.ImpactDuration.Value / 24;
                    var hours = eligibilityCheck.ImpactDuration.Value % 24;
                    Model.DurationDaysNumber = days;
                    Model.DurationDaysText = days.ToString(CultureInfo.CurrentCulture);
                    Model.DurationHoursNumber = hours;
                    Model.DurationHoursText = hours.ToString(CultureInfo.CurrentCulture);
                }
            }

            _durationOptions = await CreateDurationOptions();

            _isLoading = false;
            StateHasChanged();

            
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
        return new EligibilityCheckDto();
    }

    private async Task OnValidSubmit()
    {
        var eligibilityCheck = await GetEligibilityCheck();
        var updated = eligibilityCheck with
        {
            DurationKnownId = Model.DurationKnownId,
            ImpactDuration = Model.DurationKnownId == FloodDurationIds.DurationKnown
                ? (Model.DurationDaysNumber ?? 0) * 24 + (Model.DurationHoursNumber ?? 0)
                : null,
        };

        await protectedSessionStorage.SetAsync(SessionConstants.EligibilityCheck, updated);

        // Go to the next page or back to the summary
        var nextPage = FromSummary ? FloodReportCreatePages.Summary : FloodReportCreatePages.FloodSource;
        navigationManager.NavigateTo(nextPage.Url);
    }

    private async Task<IReadOnlyCollection<GdsOptionItem<Guid>>> CreateDurationOptions()
    {
        const string idPrefix = "duration-known";
        var floodProblems = await commonRepository.GetFloodProblemsByCategory(FloodProblemCategory.Duration, _cts.Token);
        return [.. floodProblems.Select(o => CreateOption(o, idPrefix, Model.DurationKnownId))];
    }

    private static GdsOptionItem<Guid> CreateOption(FloodProblem floodProblem, string idPrefix, Guid? selectedValue)
    {
        var id = $"{idPrefix}-{floodProblem.Id}".AsSpan();
        var label = floodProblem.TypeDescription.AsSpan();
        var selected = floodProblem.Id == selectedValue;
        var isExclusive = floodProblem.Id == FloodDurationIds.DurationNotSure;

        return new GdsOptionItem<Guid>(id, label, floodProblem.Id, selected, isExclusive);
    }
}

using FloodOnlineReportingTool.Database.Models;
using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.Public.Models;
using FloodOnlineReportingTool.Public.Models.Order;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport.Investigation;

//Reference article https://adamsilver.io/blog/designing-a-time-input/
//Further reading https://becoming-a-teacher.design-history.education.gov.uk/manage-teacher-training-applications/allowing-a-wider-range-of-input-formats-for-interview-time/
//Conclusion is to allow time to be input as a free text with wide range of values validated. 
//Note, suggestion to put AM/PM on side of time.

[Authorize]
public partial class InternalWhen(
    ILogger<Speed> logger,
    ICommonRepository commonRepository,
    ProtectedSessionStorage protectedSessionStorage,
    NavigationManager navigationManager,
    IGdsJsInterop gdsJs
) : IPageOrder, IAsyncDisposable
{
    // Page order properties
    public string Title { get; set; } = InvestigationPages.InternalWhen.Title;
    public IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [
        GeneralPages.Home.ToGdsBreadcrumb(),
        FloodReportPages.Overview.ToGdsBreadcrumb(),
        InvestigationPages.InternalHow.ToGdsBreadcrumb(),
    ];

    [SupplyParameterFromQuery]
    private bool FromSummary { get; set; }

    private Models.FloodReport.Investigation.InternalWhen Model { get; set; } = default!;

    private EditContext _editContext = default!;
    private readonly CancellationTokenSource _cts = new();
    private bool _isLoading = true;
    private IReadOnlyCollection<GdsOptionItem<Guid>> _whenWaterEnteredOptions = [];

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
            Model.WhenWaterEnteredKnownId = investigation.WhenWaterEnteredKnownId;
            if (investigation.FloodInternalUtc.HasValue)
            {
                var date = investigation.FloodInternalUtc.Value;
                var (dateOnly, timeOnly, offset) = date;
                Model.WhenWaterEnteredDate = new GdsDate(date);
                Model.TimeText = timeOnly.ToLongTimeString();
            }

            _whenWaterEnteredOptions = await CreateWhenWaterEnteredOptions();

            _isLoading = false;
            StateHasChanged();

            await gdsJs.InitGds(_cts.Token);
        }
    }

    private async Task OnSubmit()
    {
        Model.TimeText = ConvertSpecialTimeText(Model.TimeText);

        if (!_editContext.Validate())
        {
            return;
        }

        await OnValidSubmit();
    }

    // Helper to convert special values
    private static string? ConvertSpecialTimeText(string? timeText)
    {
        if (timeText == null)
        {
            return null;
        }

        var checkText = timeText.Trim().ToLowerInvariant();
        return checkText switch
        {
            "morning" => "09:00",
            "midday" or "noon" => "12:00",
            "afternoon" => "14:00",
            "midnight" => "00:00",
            _ => timeText.Trim(),
        };
    }

    private async Task OnValidSubmit()
    {
        DateTimeOffset? floodingDate = null;
        if (Model.WhenWaterEnteredKnownId.Equals(RecordStatusIds.Yes))
        {
            if (Model.WhenWaterEnteredDate.DateUtc == null)
            {
                logger.LogError("The WhenWaterEnteredDate is null. Cannot proceed with submission. This should have been caught by the Fluent Validation.");
                return;
            }
            var dateTimeOffset = Model.WhenWaterEnteredDate.DateUtc.Value;

            DateOnly dateOnly = DateOnly.FromDateTime(dateTimeOffset.DateTime);
            TimeOnly timeOnly = Model.Time ?? TimeOnly.MinValue;
            floodingDate = new DateTimeOffset(dateOnly, timeOnly, TimeSpan.Zero);
        }

        var investigation = await GetInvestigation();
        var updatedInvestigation = investigation with
        {
            WhenWaterEnteredKnownId = Model.WhenWaterEnteredKnownId,
            FloodInternalUtc = floodingDate,
        };
        await protectedSessionStorage.SetAsync(SessionConstants.Investigation, updatedInvestigation);

        // Go to the next page or back to the summary
        var nextPage = FromSummary ? InvestigationPages.Summary : InvestigationPages.PeakDepth;
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

    private async Task<IReadOnlyCollection<GdsOptionItem<Guid>>> CreateWhenWaterEnteredOptions()
    {
        var recordStatuses = await commonRepository.GetRecordStatusesByCategory(RecordStatusCategory.General, _cts.Token);
        var withoutNotSure = recordStatuses.Where(o => o.Id != RecordStatusIds.NotSure).ToList();
        return [.. withoutNotSure.Select(o => CreateOption(o, "when-entered", Model.WhenWaterEnteredKnownId))];
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

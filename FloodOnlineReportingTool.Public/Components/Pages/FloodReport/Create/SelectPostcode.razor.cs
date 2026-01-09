using FloodOnlineReportingTool.Public.Models.Order;
using FloodOnlineReportingTool.Public.State;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport.Create;

public partial class SelectPostcode(
    ILogger<SelectPostcode> logger,
    NavigationManager navigationManager,
    FloodReportCreateState createState
) : IPageOrder, IAsyncDisposable
{
    // Page order properties
    public string Title { get; set; } = FloodReportCreatePages.Postcode.Title;
    public IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [];

    [SupplyParameterFromQuery(Name = "key")]
    private long CacheKey { get; set; }

    [SupplyParameterFromQuery]
    private bool FromSummary { get; set; }

    private Models.FloodReport.Create.SelectPostcode Model { get; set; } = default!;

    private EditContext editContext = default!;
    private readonly CancellationTokenSource _cts = new();
    //private bool _isLoading = true;
    private readonly ICollection<GdsOptionItem<bool>> _postcodeKnownOptions = [
        new("postcode-known-yes", "Yes", value: true),
        new("postcode-known-no", "No", value: false),
    ];

    private readonly ICollection<GdsOptionItem<int>> TestCheckboxOptions = [
        new ("checksTypePhone", "Phone", 1),
        new ("checksTypeEmail", "Email", 2),
        new ("checksTypeText", "Text message", 3),
        new ("checksTypePost", "Post", 4),
    ];
    private string? TestChecksPhoneNumber { get; set; }

    private readonly ICollection<GdsOptionItem<int>> TestRadioOptions = [
        new ("contactTypePhone", "Phone", 1),
        new ("contactTypeEmail", "Email", 2),
        new ("contactTypeText", "Text message", 3),
        new ("contactTypePost", "Post", 4),
    ];
    private int? TestRadio { get; set; }
    private string? TestRadiosPhoneNumber { get; set; }

    private IReadOnlyCollection<GdsBreadcrumb> BuildBreadcrumbsWithQueryParameters()
    {
        var parameters = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            { "key", CacheKey },
            { "fromsummary", FromSummary ? true : null  },
        };

        var (url, title) = FloodReportCreatePages.Home;
        var previousPageUrl = navigationManager.GetUriWithQueryParameters(url, parameters);

        return [
            GeneralPages.Home.ToGdsBreadcrumb(),
            FloodReportPages.Home.ToGdsBreadcrumb(),
            new(previousPageUrl, title),
        ];
    }

    //protected override void OnInitialized()
    //{
    //    // Setup model and edit context
    //    Model ??= new();
    //    editContext = new(Model);
    //    editContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
    //}

    protected override async Task OnInitializedAsync()
    {
        logger.LogDebug("OnInitializedAsync");

        Breadcrumbs = BuildBreadcrumbsWithQueryParameters();

        // Setup model and edit context
        Model ??= new();
        editContext = new(Model);
        editContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());

        // Restore saved state (prerendering and hydration)
        if (!RendererInfo.IsInteractive)
        {
            logger.LogDebug("Prerendering");
            //await extraData.CopyFromCache(Id, _cts.Token);
        }
        Model.Postcode = createState.Postcode;
        if (Model.Postcode != null)
        {
            Model.PostcodeKnown = true;
            _postcodeKnownOptions.Single(o => o.Value).Selected = true;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        logger.LogDebug("OnAfterRenderAsync first render {FirstRender}", firstRender);

    //    var selectedCheckboxes = string.Join(", ", TestCheckboxOptions.Where(o => o.Selected).Select(o => o.Value));

    //    logger.LogDebug(
    //        "Selection state: PostcodeKnown={SelectedPostcode}, TestCheckboxes=[{SelectedCheckboxes}], TestRadio={SelectedRadioButton}",
    //        Model.PostcodeKnown,
    //        selectedCheckboxes,
    //        TestRadio
    //    );

    //    if (firstRender)
    //    {
    //        // Set any previously entered data
    //        var createExtraData = await GetCreateExtraData();
    //        Model.Postcode = createExtraData.Postcode;
    //        if (Model.Postcode != null)
    //        {
    //            Model.PostcodeKnown = true;
    //            _postcodeKnownOptions.Single(o => o.Value).Selected = true;
    //        }

    //        _isLoading = false;
    //        StateHasChanged();
    //    }
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

    private async Task OnValidSubmit()
    {
        // Save the postcode
        //var createExtraData = await GetCreateExtraData();
        //var updatedExtraData = createExtraData with
        //{
        //    Postcode = Model.Postcode?.ToUpperInvariant(),
        //};

        //await protectedSessionStorage.SetAsync(SessionConstants.EligibilityCheck_ExtraData, updatedExtraData);
        createState.Postcode = Model.Postcode?.ToUpperInvariant();
        //await extraData.SaveToCache(Id, _cts.Token);

        // Go to the next page or pass back to the summary (user must return from property type page)
        var parameters = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            { "key", CacheKey },
            { "fromsummary", FromSummary ? true : null  },
        };
        var nextPageUrl = navigationManager.GetUriWithQueryParameters(GetNextPage().Url, parameters);
        navigationManager.NavigateTo(nextPageUrl);
    }

    private PageInfo GetNextPage()
    {
        if (Model.PostcodeKnown == true)
        {
            return FloodReportCreatePages.Address;
        }

        return FloodReportCreatePages.Location;
    }

    //private async Task<ExtraData> GetCreateExtraData()
    //{
    //    var data = await protectedSessionStorage.GetAsync<ExtraData>(SessionConstants.EligibilityCheck_ExtraData);
    //    if (data.Success)
    //    {
    //        if (data.Value != null)
    //        {
    //            return data.Value;
    //        }
    //    }

    //    logger.LogWarning("Eligibility Check > Extra Data was not found in the protected storage.");
    //    return new();
    //}
}

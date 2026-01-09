namespace FloodOnlineReportingTool.Public.Components.Layout;

public partial class MainLayout(
    //ILogger<MainLayout> logger,
    IWebHostEnvironment environment
    //IGdsJsInterop gdsJsInterop,
    //NavigationManager navigationManager
) {
    private readonly Uri _feedbackUri = new("https://dorset-self.achieveservice.com/service/flood-reporting-tool-feedback");

    //protected override Task OnInitializedAsync()
    //{
    //    logger.LogDebug("OnInitializedAsync");

    //    return base.OnInitializedAsync();
    //}

    //protected override async Task OnAfterRenderAsync(bool firstRender)
    //{
    //    logger.LogDebug("OnAfterRenderAsync first render {FirstRender}", firstRender);

    //    await base.OnAfterRenderAsync(firstRender);
    //    if (firstRender)
    //    {
    //        try
    //        {
    //            logger.LogDebug("Initialising GDS for location {Location}", navigationManager.Uri);
    //            await gdsJsInterop.InitGds();
    //            navigationManager.LocationChanged += OnLocationChanged;
    //        }
    //        catch (Exception ex)
    //        {
    //            // Suppress errors if circuit disconnects
    //            logger.LogError(ex, "Error initialising GDS for location {Location}", navigationManager.Uri);
    //        }
    //    }
    //}

    //private async void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    //{
    //    try
    //    {
    //        logger.LogDebug("Location changed to {Location}, re-initialising GDS", e.Location);
    //        await gdsJsInterop.InitGds();
    //    }
    //    catch (Exception ex)
    //    {
    //        // Suppress errors if circuit disconnects during navigation
    //        logger.LogError(ex, "Error initialising GDS on location change to {Location}", e.Location);
    //    }
    //}

    //public void Dispose()
    //{
    //    navigationManager.LocationChanged -= OnLocationChanged;
    //}
}

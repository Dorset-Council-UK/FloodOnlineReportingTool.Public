using FloodOnlineReportingTool.DataAccess.Settings;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;

namespace FloodOnlineReportingTool.Public.Components;

public partial class App(IOptions<GISSettings> _settings, IJSRuntime JS)
{
    [CascadingParameter]
    private HttpContext? HttpContext { get; set; }

    // The Render Mode is InteractiveServer most of the time. Except on pages which are set as Static server-side rendering (static SSR)
    private IComponentRenderMode? PageRenderMode => HttpContext?.AcceptsInteractiveRouting() == true ? RenderMode.InteractiveServer : null;

    private readonly GISSettings _gisSettings = _settings.Value;
    private string _pathBase = "/";

    protected override void OnInitialized()
    {
        var pathBase = _gisSettings.PathBase;
        if (pathBase != null)
        {
            _pathBase = $"/{pathBase}/";
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JS.InvokeVoidAsync("window.initGDS");
        }
    }
}

using FloodOnlineReportingTool.Database.Options;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Options;

namespace FloodOnlineReportingTool.Public.Components;

public partial class App(IOptions<GISOptions> _options)
{
    [CascadingParameter]
    private HttpContext? HttpContext { get; set; }

    // The Render Mode is InteractiveServer most of the time. Except on pages which are set as Static server-side rendering (static SSR)
    private IComponentRenderMode? PageRenderMode => HttpContext?.AcceptsInteractiveRouting() == true ? RenderMode.InteractiveServer : null;

    private readonly GISOptions _gisOptions = _options.Value;
    private string _pathBase = "/";

    protected override void OnInitialized()
    {
        var pathBase = _gisOptions.PathBase;
        if (pathBase != null)
        {
            _pathBase = $"/{pathBase}/";
        }
    }

}

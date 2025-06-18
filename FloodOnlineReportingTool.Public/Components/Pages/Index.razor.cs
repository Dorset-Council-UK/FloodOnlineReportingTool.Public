using FloodOnlineReportingTool.GdsComponents;
using FloodOnlineReportingTool.Public.Models.Order;
using Microsoft.JSInterop;

namespace FloodOnlineReportingTool.Public.Components.Pages;

public partial class Index(IJSRuntime JS) : IPageOrder
{
    // Page order properties
    public string Title { get; set; } = GeneralPages.Home.Title;
    public IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [];

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JS.InvokeVoidAsync("window.initGDS");
        }
    }
}

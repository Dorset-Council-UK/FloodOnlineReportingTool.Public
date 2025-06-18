using FloodOnlineReportingTool.GdsComponents;
using FloodOnlineReportingTool.Public.Models.Order;
using Microsoft.JSInterop;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport;

public partial class Index(IJSRuntime JS) : IPageOrder
{
    // Page order properties
    public string Title { get; set; } = FloodReportPages.Home.Title;
    public IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [ GeneralPages.Home.ToGdsBreadcrumb() ];

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JS.InvokeVoidAsync("window.initGDS");
        }
    }
}

using FloodOnlineReportingTool.Public.Models.Order;
using GdsBlazorComponents;

namespace FloodOnlineReportingTool.Public.Components.Pages;

public partial class Index() : IPageOrder
{
    // Page order properties
    public string Title { get; set; } = GeneralPages.Home.Title;
    public IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [];

}

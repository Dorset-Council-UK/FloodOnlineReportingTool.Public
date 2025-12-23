using FloodOnlineReportingTool.Public.Models.Order;
using GdsBlazorComponents;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport;

public partial class Index() : IPageOrder
{
    // Page order properties
    public string Title { get; set; } = FloodReportPages.Home.Title;
    public IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [GeneralPages.Home.ToGdsBreadcrumb()];

}

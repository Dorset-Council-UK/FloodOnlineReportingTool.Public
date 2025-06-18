using FloodOnlineReportingTool.GdsComponents;

namespace FloodOnlineReportingTool.Public.Models.Order;

internal interface IPageOrder
{
    string Title { get; set; }
    IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; }
}

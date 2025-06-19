using FloodOnlineReportingTool.Public.Models.Order;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using System.Diagnostics;

namespace FloodOnlineReportingTool.Public.Components.Pages;

public partial class Error(ILogger<Error> logger) : IPageOrder
{
    // Page order properties
    public string Title { get; set; } = GeneralPages.Error.Title;
    public IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [];

    [CascadingParameter]
    private HttpContext? HttpContext { get; set; }

    private string? RequestId { get; set; }
    private bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    private readonly Uri? _contactLink = new("https://dorset-self.achieveservice.com/service/flood-reporting-tool-feedback");
    private readonly string _contactText = "let us know.";

    protected override void OnInitialized()
    {
        logger.LogError("Error page initialized. RequestId: {RequestId}", RequestId);
        RequestId = Activity.Current?.Id ?? HttpContext?.TraceIdentifier;
    }
}

using FloodOnlineReportingTool.Database.Models.Eligibility;
using FloodOnlineReportingTool.Database.Models.Investigate;
using FloodOnlineReportingTool.Database.Models.Status;
using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.Public.Models;
using FloodOnlineReportingTool.Public.Models.FloodReport.Investigation;
using FloodOnlineReportingTool.Public.Models.Order;
using FluentValidation;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Globalization;
using System.Security.Claims;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport.Investigation;

[Authorize]
public partial class PeakDepth
{
    // Page order properties
    [Parameter]
    public EventCallback<Models.FloodReport.Investigation.PeakDepth> OnAdd { get; set; }


    private Models.FloodReport.Investigation.PeakDepth Model = new ();

    private EditContext _editContext = default!;
    private readonly CancellationTokenSource _cts = new();
    private bool _isLoading = true;




    private async Task OnValidSubmit()
    {

        await OnAdd.InvokeAsync(Model);

        Model = new();

    }
}

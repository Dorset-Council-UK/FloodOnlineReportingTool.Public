using FloodOnlineReportingTool.Database.Models.Eligibility;
using FloodOnlineReportingTool.Database.Models.Investigate;
using FloodOnlineReportingTool.Public.Models;
using FloodOnlineReportingTool.Public.Models.FloodReport.Create;
using FloodOnlineReportingTool.Public.Models.Order;
using FloodOnlineReportingTool.Public.Services;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace FloodOnlineReportingTool.Public.Components.Pages;

public partial class Test(
    ProtectedSessionStorage protectedSessionStorage,
    IGdsJsInterop gdsJs,
    TestService testService,
    IGovNotifyEmailSender govNotifyEmailSender,
    IConfiguration Configuration
) : IPageOrder, IAsyncDisposable
{
    // Page order properties
    public string Title { get; set; } = GeneralPages.Test.Title;
    public IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [];

    private readonly IReadOnlyCollection<PageInfoWithNote> _floodReportCreatePages = [
        new (FloodReportCreatePages.Home),
        new (FloodReportCreatePages.Location, "(optional*)"),
        new (FloodReportCreatePages.Address, "(optional*)"),
        new (FloodReportCreatePages.PropertyType),
        new (FloodReportCreatePages.FloodAreas),
        new (FloodReportCreatePages.TemporaryPostcode, "(optional*)"),
        new (FloodReportCreatePages.TemporaryAddress, "(optional*)"),
        new (FloodReportCreatePages.Vulnerability),
        new (FloodReportCreatePages.FloodStarted),
        new (FloodReportCreatePages.FloodDuration, "(optional)"),
        new (FloodReportCreatePages.FloodSource),
        new (FloodReportCreatePages.FloodSecondarySource, "(optional)"),
        new (FloodReportCreatePages.Summary),
        new (FloodReportCreatePages.Confirmation),
    ];

    private readonly IReadOnlyCollection<PageInfoWithNote> _investigationPages = [
        new (InvestigationPages.Home),
        new (InvestigationPages.Speed),
        new (InvestigationPages.Destination),
        new (InvestigationPages.Vehicles),
        new (InvestigationPages.InternalHow, "(optional)"),
        new (InvestigationPages.InternalWhen, "(optional)"),
        new (InvestigationPages.PeakDepth),
        new (InvestigationPages.CommunityImpact),
        new (InvestigationPages.Blockages),
        new (InvestigationPages.ActionsTaken),
        new (InvestigationPages.HelpReceived),
        new (InvestigationPages.Warnings),
        new (InvestigationPages.WarningSources),
        new (InvestigationPages.Floodline, "(optional)"),
        new (InvestigationPages.History),
        new (InvestigationPages.Summary),
        new (InvestigationPages.Confirmation),
    ];

    private readonly IReadOnlyCollection<PageInfoWithNote> _accountPages = [
        new (AccountPages.SignIn),
        new (new($"{AccountPages.SignOut.Url}?returnUrl={GeneralPages.Test.Url}", AccountPages.SignOut.Title)),
        new (AccountPages.MyAccount)
    ];

    private readonly CancellationTokenSource _cts = new();
    private bool _hasCreateData;
    private bool _hasCreateExtraData;
    private bool _hasInvestigationData;

    public async ValueTask DisposeAsync()
    {
        try
        {
            await _cts.CancelAsync();
            _cts.Dispose();
        }
        catch (Exception)
        {
        }

        GC.SuppressFinalize(this);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _hasCreateData = await HasCreateData();
            _hasCreateExtraData = await HasCreateExtraData();
            _hasInvestigationData = await HasInvestigationData();

            await gdsJs.InitGds(_cts.Token);
        }
    }

    private async Task<bool> HasCreateData()
    {
        var data = await protectedSessionStorage.GetAsync<EligibilityCheckDto?>(SessionConstants.EligibilityCheck);
        return data.Success && data.Value != null;
    }
    private async Task<bool> HasCreateExtraData()
    {
        var data = await protectedSessionStorage.GetAsync<ExtraData?>(SessionConstants.EligibilityCheck_ExtraData);
        return data.Success && data.Value != null;
    }
    private async Task<bool> HasInvestigationData()
    {
        var data = await protectedSessionStorage.GetAsync<InvestigationDto?>(SessionConstants.Investigation);
        return data.Success && data.Value != null;
    }

    private async Task<bool> TestNotifcation()
    {
        var testEmail = Configuration["GovNotify:TestEmail"];
        if (testEmail == null)
        {
            return false;
        }
        var result = await govNotifyEmailSender.SendTestNotification(testEmail, "This is a test of the FORT notification system - public reporting project.", _cts.Token).ConfigureAwait(false);
        return String.IsNullOrEmpty(result)!;
    }

    private async Task TestMessage()
    {
        await testService.TestMessage(_cts.Token);
    }
    private async Task BlankCreateData()
    {
        await protectedSessionStorage.SetAsync(SessionConstants.EligibilityCheck, new EligibilityCheckDto());
        _hasCreateData = await HasCreateData();
    }
    private async Task BlankCreateExtraData()
    {
        await protectedSessionStorage.SetAsync(SessionConstants.EligibilityCheck_ExtraData, new ExtraData());
        _hasCreateExtraData = await HasCreateExtraData();
    }
    private async Task BlankInvestigationData()
    {
        await protectedSessionStorage.SetAsync(SessionConstants.Investigation, new InvestigationDto());
        _hasInvestigationData = await HasInvestigationData();
    }

    private async Task DeleteCreateData()
    {
        await protectedSessionStorage.DeleteAsync(SessionConstants.EligibilityCheck);
        _hasCreateData = await HasCreateData();
    }
    private async Task DeleteCreateExtraData()
    {
        await protectedSessionStorage.DeleteAsync(SessionConstants.EligibilityCheck_ExtraData);
        _hasCreateExtraData = await HasCreateExtraData();
    }
    private async Task DeleteInvestigationData()
    {
        await protectedSessionStorage.DeleteAsync(SessionConstants.Investigation);
        _hasInvestigationData = await HasInvestigationData();
    }
}
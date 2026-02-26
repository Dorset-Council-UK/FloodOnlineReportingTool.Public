using FloodOnlineReportingTool.Contracts.Shared;
using FloodOnlineReportingTool.Database.Models.Contact;
using FloodOnlineReportingTool.Database.Models.Contact.Subscribe;
using FloodOnlineReportingTool.Database.Models.Eligibility;
using FloodOnlineReportingTool.Database.Models.Investigate;
using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.Public.Models;
using FloodOnlineReportingTool.Public.Models.FloodReport.Create;
using FloodOnlineReportingTool.Public.Models.Order;
using FloodOnlineReportingTool.Public.Services;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Security.Claims;

namespace FloodOnlineReportingTool.Public.Components.Pages;

public partial class Test(
    ILogger<Test> logger,
    ProtectedSessionStorage protectedSessionStorage,
    TestService testService,
    IGovNotifyEmailSender govNotifyEmailSender,
    IConfiguration Configuration,
    IContactRecordRepository contactRepository,
    IFloodReportRepository floodReportRepository,
    NavigationManager navigationManager
) : IPageOrder, IAsyncDisposable
{
    // Page order properties
    public string Title { get; set; } = GeneralPages.Test.Title;
    public IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [];

    [CascadingParameter]
    public Task<AuthenticationState>? AuthenticationState { get; set; }

    private readonly IReadOnlyCollection<PageInfoWithNote> _floodReportCreatePages = [
        new (FloodReportCreatePages.Home),
        new (FloodReportCreatePages.Location, "(optional <sup>[1]</sup>)"),
        new (FloodReportCreatePages.Address, "(optional <sup>[1]</sup>)"),
        new (FloodReportCreatePages.PropertyType),
        new (FloodReportCreatePages.FloodAreas),
        new (FloodReportCreatePages.TemporaryPostcode, "(optional <sup>[1]</sup>)"),
        new (FloodReportCreatePages.TemporaryAddress, "(optional <sup>[1]</sup>)"),
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
        new (InvestigationPages.ServiceImpact),
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
        new (AccountPages.MyAccount),
    ];

    private readonly CancellationTokenSource _cts = new();
    private bool _hasCreateData;
    private bool _hasCreateExtraData;

    // Investigation
    private bool _investigationInProtectedStorage;
    private bool? _investigationHasStarted;
    private string? _floodReportStatusText;

    private string SignInUrl
    {
        get
        {
            var redirectUri = navigationManager.SignInRedirectUri;
            return string.IsNullOrWhiteSpace(redirectUri) ? "signin" : $"signin?redirectUri={redirectUri}";
        }
    }

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
            await TestRedaction();

            _hasCreateData = await HasCreateData();
            _hasCreateExtraData = await HasCreateExtraData();

            // investigation
            var yourLastFloodReport = await GetYourLastFloodReport();
            if (yourLastFloodReport is null)
            {
                _investigationHasStarted = null;
                _floodReportStatusText = null;
            }
            else
            {
                _investigationHasStarted = floodReportRepository.HasInvestigationStarted(yourLastFloodReport.StatusId);
                _floodReportStatusText = yourLastFloodReport.Status?.Text;
            }
            _investigationInProtectedStorage = await InvestigationExistsInProtectedStorage();

            StateHasChanged();
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

    private async Task<bool> TestNotifcation()
    {
        var testEmail = Configuration["GovNotify:TestEmail"];
        if (testEmail == null)
        {
            return false;
        }
        var result = await govNotifyEmailSender.SendTestNotification(testEmail, "This is a test of the FORT notification system - public reporting project.", _cts.Token);
        return string.IsNullOrEmpty(result)!;
    }

    private async Task TestMessage()
    {
        await testService.TestMessage(_cts.Token);
    }
    public async Task TestFloodReport()
    {
        var reference = await testService.TestFloodReport(_cts.Token)
            ?? throw new InvalidOperationException("TestFloodReport did not work, investigate.");

        navigationManager.NavigateTo($"{FloodReportCreatePages.Confirmation.Url}?reference={reference}");
    }

    private async Task TestRedaction()
    {
        // Test with fake data - should return consistent output
        SubscribeRecord testSubscriber = new()
        {
            Id = Guid.NewGuid(),
            IsRecordOwner = true,
            ContactType = ContactRecordType.Tenant,
            ContactName = "Test User",
            EmailAddress = "test@email.com",
            PhoneNumber = "01234567890",
            IsEmailVerified = true,
            IsSubscribed = true,
            CreatedUtc = DateTimeOffset.UtcNow,
            RedactionDate = DateTimeOffset.UtcNow.AddYears(1),
        };

        ContactRecord testContact = new()
        {
            Id = Guid.NewGuid(),
            CreatedUtc = DateTimeOffset.UtcNow,
            RedactionDate = DateTimeOffset.UtcNow.AddYears(1),
            SubscribeRecords = [testSubscriber],
        };

        logger.LogSubscriberRecord(testSubscriber);
        logger.LogInformation("Contact Record {owner}", testContact);

        // Trigger the redaction on real data
        var testId = await contactRepository.GetRandomFloodReportWithSubscriber(_cts.Token);
        var contact = await contactRepository.GetReportOwnerContactByReport(testId, _cts.Token);
        if (contact is not null)
        {
            logger.LogDebug("Testing redaction on contact with id {contactId}", contact.Id);
        }
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

    // Investigation actions
    private async Task<bool> InvestigationExistsInProtectedStorage()
    {
        var data = await protectedSessionStorage.GetAsync<InvestigationDto?>(SessionConstants.Investigation);
        return data.Success && data.Value is not null;
    }
    private async Task InvestigationRemove()
    {
        await protectedSessionStorage.DeleteAsync(SessionConstants.Investigation);
        _investigationInProtectedStorage = await InvestigationExistsInProtectedStorage();
    }
    private async Task InvestigationAddEmpty()
    {
        await protectedSessionStorage.SetAsync(SessionConstants.Investigation, new InvestigationDto());
        _investigationInProtectedStorage = await InvestigationExistsInProtectedStorage();
    }
    private async Task InvestigationAddTest()
    {
        var investigationDto = await testService.TestInvestigationDto(_cts.Token);
        if (investigationDto is null)
        {
            logger.LogError("TestInvestigationDto returned null, cannot add to protected storage.");
            return;
        }

        await protectedSessionStorage.SetAsync(SessionConstants.Investigation, investigationDto);
        _investigationInProtectedStorage = await InvestigationExistsInProtectedStorage();
    }
    private async Task InvestigationActionNeededStatus()
    {
        var floodReport = await GetYourLastFloodReport();
        if (floodReport is null)
        {
            return;
        }
        await testService.TestFloodReportActionNeededStatus(floodReport.Id, _cts.Token);
        StateHasChanged();
    }
    private async Task<Database.Models.Flood.FloodReport?> GetYourLastFloodReport()
    {
        string? objectId = null;
        if (AuthenticationState is not null)
        {
            var authState = await AuthenticationState;
            objectId = authState.User.Oid;
        }

        if (objectId is null)
        {
            return null;
        }

        // TODO: Work out what to do when the user has reported multiple floods
        var floodReports = await floodReportRepository.AllReportedByContact(objectId, _cts.Token);
        return floodReports.OrderByDescending(o => o.CreatedUtc).FirstOrDefault();
    }
}
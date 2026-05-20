using FloodOnlineReportingTool.Contracts.Shared;
using FloodOnlineReportingTool.Database.Models.Contact;
using FloodOnlineReportingTool.Database.Models.Contact.Subscribe;
using FloodOnlineReportingTool.Database.Models.Eligibility;
using FloodOnlineReportingTool.Database.Models.Investigate;
using FloodOnlineReportingTool.Database.Models.Messaging;
using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.Database.Services;
using FloodOnlineReportingTool.Public.Authentication;
using FloodOnlineReportingTool.Public.Models;
using FloodOnlineReportingTool.Public.Models.FloodReport.Create;
using FloodOnlineReportingTool.Public.Models.Order;
using FloodOnlineReportingTool.Public.Options;
using FloodOnlineReportingTool.Public.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace FloodOnlineReportingTool.Public.Components.Pages;

public partial class Test(
    ILogger<Test> logger,
    IAuthorizationService authorizationService,
    ProtectedSessionStorage protectedSessionStorage,
    ITestService testService,
    IGovNotifyEmailSender govNotifyEmailSender,
    IContactRecordRepository contactRepository,
    IFloodReportRepository floodReportRepository,
    IOutboxMessageService outboxMessageService,
    IOptions<GovNotifyOptions> govNotifyOptions,
    NavigationManager navigationManager
) : IAsyncDisposable
{
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

    // Outbox messages
    private int _outboxMessageCount;
    private int _outboxMessageCountPending;
    private int _outboxMessageCountProcessed;
    private int _outboxMessageCountFailed;

    // Notifications
    private string? _notificationTestEmailAddress;
    private bool _notificationHasTestNotification;

    private string SignInUrl
    {
        get
        {
            var redirectUri = navigationManager.SignInRedirectUri;
            return string.IsNullOrWhiteSpace(redirectUri) ? AccountPages.SignIn.Url : $"{AccountPages.SignIn.Url}?redirectUri={redirectUri}";
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

    protected override async Task OnInitializedAsync()
    {
        var hasAdminPolicy = await HasPolicy(PolicyNames.Admin);
        if (hasAdminPolicy)
        {
            var options = govNotifyOptions.Value;
            _notificationTestEmailAddress = options.TestEmail;
            _notificationHasTestNotification = !string.IsNullOrWhiteSpace(options.Templates.TestNotification);

            await OutboxMessage_GetCounts();
        }
    }

    private async Task<bool> HasPolicy(string policyName)
    {
        if (AuthenticationState is null)
        {
            return false;
        }

        var authState = await AuthenticationState;
        if (!authState.User.IsAuthenticated)
        {
            return false;
        }

        var policyCheck = await authorizationService.AuthorizeAsync(authState.User, policyName);
        return policyCheck.Succeeded;
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

    private async Task Notifcation_SendTest()
    {
        if (!string.IsNullOrWhiteSpace(_notificationTestEmailAddress))
        {
            _ = await govNotifyEmailSender.SendTestNotification(_notificationTestEmailAddress, "This is a test of the FORT notification system - public reporting project.", _cts.Token);
        }
    }

    public async Task FloodReport_Create()
    {
        var floodReport = await testService.TestFloodReport_Create(_cts.Token)
            ?? throw new InvalidOperationException("Creating a test flood report did not work, investigate.");
        navigationManager.NavigateTo($"{FloodReportCreatePages.Confirmation.Url}?reference={floodReport.Reference}");
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
        if (AuthenticationState is null)
        {
            return null;
        }

        var authState = await AuthenticationState;
        if (!authState.User.IsAuthenticated)
        {
            return null;
        }

        string? userId = authState.User.Oid;
        if (userId is null)
        {
            return null;
        }

        // TODO: Work out what to do when the user has reported multiple floods
        var floodReports = await floodReportRepository.ReportedByUser(userId, _cts.Token);
        return floodReports.FirstOrDefault();
    }

    private async Task OutboxMessage_GetCounts()
    {
        _outboxMessageCount = await outboxMessageService.Count(_cts.Token);
        _outboxMessageCountPending = await outboxMessageService.Count(MessageStatus.Pending, _cts.Token);
        _outboxMessageCountProcessed = await outboxMessageService.Count(MessageStatus.Processed, _cts.Token);
        _outboxMessageCountFailed = await outboxMessageService.Count(MessageStatus.Failed, _cts.Token);
    }

    private async Task OutboxMessage_FloodReportSourceCreated_Add(MessageStatus messageStatus)
    {
        _ = await testService.TestOutboxMessage_FloodReportSourceCreated(messageStatus, _cts.Token)
            ?? throw new Exception("Failed to create a test outbox message.");
        await OutboxMessage_GetCounts();
    }

    private async Task OutboxMessage_FloodReportSourceUpdated_Add(MessageStatus messageStatus)
    {
        _ = await testService.TestOutboxMessage_FloodReportSourceUpdated(messageStatus, _cts.Token)
            ?? throw new Exception("Failed to create a test outbox message.");
        await OutboxMessage_GetCounts();
    }
}
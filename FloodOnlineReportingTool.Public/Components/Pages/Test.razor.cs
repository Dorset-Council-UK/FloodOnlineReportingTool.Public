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

    // Protected storage
    private record ProtectedStorageInfo(string Label)
    {
        internal bool Exists { get; set; }
    };
    private readonly Dictionary<string, ProtectedStorageInfo> _protectedStorageInfos = new(StringComparer.Ordinal)
    {
        { SessionConstants.FloodReportId, new ProtectedStorageInfo("Flood report ID") },
        { SessionConstants.EligibilityCheck, new ProtectedStorageInfo("Eligibility check") },
        { SessionConstants.EligibilityCheck_ExtraData, new ProtectedStorageInfo("Eligibility check extra data") },
        { SessionConstants.Investigation, new ProtectedStorageInfo("Investigation") },
        { SessionConstants.VerificationId, new ProtectedStorageInfo("Verification ID") },
    };

    // Flood reports
    private int _floodReportsCount;
    private int _floodReportsCountUser;
    private Database.Models.Flood.FloodReport? _floodReportsYourLast;
    private string? FloodReportsYourLast_StatusText => _floodReportsYourLast?.Status?.Text;

    // Investigation
    private bool? _investigationHasStarted;

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
        // Admin policy check
        if (AuthenticationState is not null)
        {
            var authState = await AuthenticationState;
            if (authState.User.IsAuthenticated)
            {
                var adminPolicyResult = await authorizationService.AuthorizeAsync(authState.User, PolicyNames.Admin);
                if (adminPolicyResult.Succeeded)
                {
                    string? userId = authState.User.Oid;

                    // Protected storage
                    await ProtectedStorage_Refresh();

                    // Flood reports
                    await FloodReportsCounts_Refresh(userId);
                    if (userId is not null)
                    {
                        _floodReportsYourLast = await testService.TestFloodReport_GetLast(userId, _cts.Token);
                    }

                    // Investigation
                    if (_floodReportsYourLast is not null)
                    {
                        _investigationHasStarted = floodReportRepository.HasInvestigationStarted(_floodReportsYourLast.StatusId);
                    }

                    // Outbox messages
                    await OutboxMessage_GetCounts();

                    // Notifications
                    var options = govNotifyOptions.Value;
                    _notificationTestEmailAddress = options.TestEmail;
                    _notificationHasTestNotification = !string.IsNullOrWhiteSpace(options.Templates.TestNotification);
                }
            }
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            //// Investigation
            //var yourLastFloodReport = await GetYourLastFloodReport();
            //if (yourLastFloodReport is null)
            //{
            //    _investigationHasStarted = null;
            //    _floodReportStatusText = null;
            //}
            //else
            //{
            //    _investigationHasStarted = floodReportRepository.HasInvestigationStarted(yourLastFloodReport.StatusId);
            //    _floodReportStatusText = yourLastFloodReport.Status?.Text;
            //}

            // Notifications
            await TestRedaction();

            StateHasChanged();
        }
    }

    private async Task Notifcation_SendTest()
    {
        if (!string.IsNullOrWhiteSpace(_notificationTestEmailAddress))
        {
            _ = await govNotifyEmailSender.SendTestNotification(_notificationTestEmailAddress, "This is a test of the FORT notification system - public reporting project.", _cts.Token);
        }
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

    // Protected storage
    private async Task ProtectedStorage_Refresh()
    {
        foreach (var info in _protectedStorageInfos)
        {
            if (info.Key is SessionConstants.FloodReportId)
            {
                var result = await protectedSessionStorage.GetAsync<Guid?>(info.Key);
                info.Value.Exists = result.Success && result.Value is not null && result.Value != Guid.Empty;
            }
            else if (info.Key is SessionConstants.EligibilityCheck)
            {
                var result = await protectedSessionStorage.GetAsync<EligibilityCheckDto?>(info.Key);
                info.Value.Exists = result.Success && result.Value is not null;
            }
            else if (info.Key is SessionConstants.EligibilityCheck_ExtraData)
            {
                var result = await protectedSessionStorage.GetAsync<ExtraData?>(info.Key);
                info.Value.Exists = result.Success && result.Value is not null;
            }
            else if (info.Key is SessionConstants.Investigation)
            {
                var result = await protectedSessionStorage.GetAsync<InvestigationDto?>(info.Key);
                info.Value.Exists = result.Success && result.Value is not null;
            }
            else if (info.Key is SessionConstants.VerificationId)
            {
                var result = await protectedSessionStorage.GetAsync<Guid?>(info.Key);
                info.Value.Exists = result.Success && result.Value is not null && result.Value != Guid.Empty;
            }
            else
            {
                throw new InvalidOperationException($"Cannot refresh protected storage information for key {info.Key}");
            }
        }
    }
    private async Task ProtectedStorage_Delete(string key)
    {
        await protectedSessionStorage.DeleteAsync(key);
        await ProtectedStorage_Refresh();
    }
    private async Task ProtectedStorage_Delete(string[] keys)
    {
        foreach (var key in keys)
        {
            await protectedSessionStorage.DeleteAsync(key);
        }
        await ProtectedStorage_Refresh();
    }
    private async Task ProtectedStorage_Create(string key)
    {
        if (key is SessionConstants.FloodReportId)
        {
            await protectedSessionStorage.SetAsync(key, _floodReportsYourLast?.Id ?? Guid.NewGuid());
        }
        else if (key is SessionConstants.EligibilityCheck)
        {
            await protectedSessionStorage.SetAsync(key, testService.TestData_EligibilityCheckDto);
        }
        else if (key is SessionConstants.EligibilityCheck_ExtraData)
        {
            await protectedSessionStorage.SetAsync(key, testService.TestData_EligibilityCheck_ExtraData);
        }
        else if (key is SessionConstants.Investigation)
        {
            await protectedSessionStorage.SetAsync(key, testService.TestData_InvestigationDto);
        }
        else if (key is SessionConstants.VerificationId)
        {
            await protectedSessionStorage.SetAsync(key, Guid.NewGuid());
        }
        else
        {
            throw new InvalidOperationException($"Cannot create protected storage information for key {key}");
        }

        await ProtectedStorage_Refresh();
    }
    private async Task ProtectedStorage_Create(string[] keys)
    {
        foreach (var key in keys)
        {
            await ProtectedStorage_Create(key);
        }
    }

    // Flood reports
    private async Task FloodReportsCounts_Refresh(string? userId)
    {
        _floodReportsCount = await floodReportRepository.Count(_cts.Token);
        _floodReportsCountUser = userId is not null
            ? await floodReportRepository.Count(userId, _cts.Token)
            : 0;
    }
    private async Task FloodReports_CreateTest()
    {
        _floodReportsYourLast = await testService.TestFloodReport_Create(_cts.Token);

        string? userId = null;

        var contactRecord = await testService.TestContactRecord_Create(_floodReportsYourLast.Id, userId, _cts.Token);

        await FloodReportsCounts_Refresh(userId);
    }

    // Investigation
    private async Task Investigation_ActionNeededStatus()
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

    // Outbox messages
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
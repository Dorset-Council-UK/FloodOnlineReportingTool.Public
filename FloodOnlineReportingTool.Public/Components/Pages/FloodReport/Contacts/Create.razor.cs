using FloodOnlineReportingTool.Database.Models;
using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.Public.Models.FloodReport.Contact;
using FloodOnlineReportingTool.Public.Models.Order;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Azure.Amqp.Framing;
using System.Reflection.Emit;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport.Contacts;

public partial class Create(
    ILogger<Create> logger,
    NavigationManager navigationManager,
    IContactRecordRepository contactRepository,
    IGdsJsInterop gdsJs
) : IPageOrder, IAsyncDisposable
{
    // Page order properties
    public string Title { get; set; } = ContactPages.Create.Title;
    public IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [
        GeneralPages.Home.ToGdsBreadcrumb(),
        FloodReportPages.Overview.ToGdsBreadcrumb(),
        ContactPages.Home.ToGdsBreadcrumb(),
    ];

    [CascadingParameter]
    public EditContext EditContext { get; set; } = default!;

    [CascadingParameter]
    public Task<AuthenticationState>? AuthenticationState { get; set; }

    private ContactModel? _contactModel;
    private EditContext _editContext = default!;
    private IReadOnlyCollection<GdsOptionItem<ContactRecordType>> _contactTypes = [];
    private ValidationMessageStore _messageStore = default!;
    private readonly CancellationTokenSource _cts = new();

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
        // Setup model and edit context
        if (_contactModel == null)
        {
            _contactModel = new();
            _editContext = new(_contactModel);
            _editContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
            _messageStore = new(_editContext);
            _contactTypes = await CreateContactTypeOptions();
        }


        // Check if user is authenticated
        if (AuthenticationState is not null)
        {

            var authState = await AuthenticationState;
            var user = authState.User;

            if (user.Identity?.IsAuthenticated ?? false)
            {
                // Populate model with known user info
                _contactModel.ContactName = string.IsNullOrWhiteSpace(_contactModel.ContactName) ? user.Identity.Name : _contactModel.ContactName;
                var oidClaim = user.FindFirst("oid")?.Value;
                _contactModel.Oid = Guid.TryParse(oidClaim, out var parsedOid) ? parsedOid : null;

                // Example: populate email if available
                var email = user.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                if (!string.IsNullOrWhiteSpace(email))
                {
                    _contactModel.EmailAddress = string.IsNullOrWhiteSpace(_contactModel.EmailAddress) ? email : _contactModel.EmailAddress ;
                }

            }

        }
    }

    private async Task<IReadOnlyCollection<GdsOptionItem<ContactRecordType>>> CreateContactTypeOptions()
    {
        ContactRecordType contactRecordType = ContactRecordType.All
        var id = contactRecordType.ToString().AsSpan();
        var label = contactRecordType is ContactRecordType.NonResident ? "Non resident".AsSpan() : id;
        var selected = contactRecordType == Contact!.ContactType;

        new GdsOptionItem<ContactRecordType>(id, label, contactRecordType, selected)

        var userId = await AuthenticationState.IdentityUserId();
        if (userId == null)
        {
            return [];
        }

        IList<ContactRecordType> unusedRecordTypes = await contactRepository.GetUnusedRecordTypes(userId.Value, _cts.Token);
        if (Contact.Id != null)
        {
            unusedRecordTypes.Add(Contact.ContactType.Value);
        }
        return [.. unusedRecordTypes.Select(CreateOption)];
    }

    private GdsOptionItem<ContactRecordType> CreateOption(ContactRecordType contactRecordType)
    {
        var id = contactRecordType.ToString().AsSpan();
        var label = contactRecordType is ContactRecordType.NonResident ? "Non resident".AsSpan() : id;
        var selected = contactRecordType == Contact!.ContactType;

        return new GdsOptionItem<ContactRecordType>(id, label, contactRecordType, selected);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await gdsJs.InitGds(_cts.Token);
        }
    }

    private async Task OnSubmit()
    {
        _messageStore.Clear();

        if (!_editContext.Validate())
        {
            return;
        }

        await CreateContact();
    }

    private async Task CreateContact()
    {
        logger.LogDebug("Creating contact information");
        try
        {
            var userId = await AuthenticationState.IdentityUserId();
            ContactRecordDto dto = new()
            {
                ContactType = _contactModel.ContactType.Value,
                ContactName = _contactModel.ContactName,
                EmailAddress = _contactModel.EmailAddress,
                PhoneNumber = _contactModel.PhoneNumber,
            };
            await contactRepository.CreateForUser(userId.Value, dto, _cts.Token);
            logger.LogInformation("Contact information created successfully for user {UserId}", userId);
            navigationManager.NavigateTo(ContactPages.Home.Url);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "There was a problem creating contact information");
            _messageStore.Add(_editContext.Field(nameof(_contactModel.ContactType)), $"There was a problem creating the contact information. Please try again but if this issue happens again then please report a bug.");
            _editContext.NotifyValidationStateChanged();
        }
    }
}

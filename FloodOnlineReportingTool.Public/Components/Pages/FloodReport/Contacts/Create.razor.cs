using FloodOnlineReportingTool.Database.Models;
using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.Public.Models.FloodReport.Contact;
using FloodOnlineReportingTool.Public.Models.Order;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport.Contacts;

public partial class Create(
    ILogger<Create> logger,
    NavigationManager navigationManager,
    IContactRecordRepository contactRepository,
    IEligibilityCheckRepository eligibilityRepository,
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

    [SupplyParameterFromQuery]
    private string? Reference { get; set; }

    [CascadingParameter]
    public EditContext EditContext { get; set; } = default!;

    [CascadingParameter]
    public Task<AuthenticationState>? AuthenticationState { get; set; }

    public IReadOnlyCollection<GdsOptionItem<ContactRecordType>> ContactTypes = [];

    private ContactModel? _contactModel;
    private bool _isLoading = true;
    private bool _loadingError;
    private Guid? _reportId;
    private EditContext _editContext = default!;
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
            ContactTypes = CreateContactTypeOptions();
        }
        // TODO - how were we passing the flood report reference here? Pick this up tomorrow
        if (!string.IsNullOrWhiteSpace(Reference))
        {
            try
            {
               var result = await eligibilityRepository.CalculateEligibilityWithReference(Reference, _cts.Token);
               ContextBoundObject.
               _reportId = result.f; 
            }
            catch (InvalidOperationException ex)
            {
                logger.LogError(ex, "There was a problem getting the eligibility check from the database");
                _loadingError = true;
            }
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

    private IReadOnlyCollection<GdsOptionItem<ContactRecordType>> CreateContactTypeOptions()
    {

        var allTypes = Enum.GetValues<ContactRecordType>();

        return [.. allTypes.Select(CreateOption)];
    }

    private GdsOptionItem<ContactRecordType> CreateOption(ContactRecordType contactRecordType)
    {
        var id = contactRecordType.ToString().AsSpan();
        var label = contactRecordType is ContactRecordType.NonResident ? "Non resident".AsSpan() : id;

        return new GdsOptionItem<ContactRecordType>(id, label, contactRecordType, false);
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
                UserId = userId.Value,
                ContactType = _contactModel.ContactType.Value,
                ContactName = _contactModel.ContactName,
                EmailAddress = _contactModel.EmailAddress,
                PhoneNumber = _contactModel.PhoneNumber,
            };
            await contactRepository.CreateForReport(, dto, _cts.Token);
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

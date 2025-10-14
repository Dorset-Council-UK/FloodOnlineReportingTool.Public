using FloodOnlineReportingTool.Database.Models;
using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.Public.Models.FloodReport.Contact;
using FloodOnlineReportingTool.Public.Models.Order;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport.Contacts;

[Authorize]
public partial class Change(
    ILogger<Change> logger,
    NavigationManager navigationManager,
    IContactRecordRepository contactRepository,
    IGdsJsInterop gdsJs
) : IPageOrder, IAsyncDisposable
{
    // Page order properties
    public string Title { get; set; } = ContactPages.Change.Title;
    public IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [
        GeneralPages.Home.ToGdsBreadcrumb(),
        FloodReportPages.Overview.ToGdsBreadcrumb(),
        ContactPages.Home.ToGdsBreadcrumb(),
    ];

    [Parameter]
    public Guid ContactId { get; set; }

    [CascadingParameter]
    public Task<AuthenticationState>? AuthenticationState { get; set; }

    private ContactModel? _contactModel;
    private EditContext _editContext = default!;
    private ValidationMessageStore _messageStore = default!;
    private readonly CancellationTokenSource _cts = new();
    private Guid _userId;

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
            _userId = await AuthenticationState.IdentityUserId() ?? Guid.Empty;
            _contactModel = await GetContact();

            if (_contactModel != null)
            {
                _editContext = new(_contactModel);
                _editContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
                _messageStore = new(_editContext);
            }
        }
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

        await UpdateContact();
    }

    private async Task UpdateContact()
    {
        logger.LogDebug("Updating contact information");

        if (_contactModel == null)
        {
            return;
        }

        try
        {
            var dto = _contactModel.ToDto();
            await contactRepository.UpdateForUser(_userId, _contactModel.Id!.Value, dto, _cts.Token);
            logger.LogInformation("Contact information updated successfully for user {UserId}", _userId);
            navigationManager.NavigateTo(ContactPages.Home.Url);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "There was a problem updating contact information");
            _messageStore.Add(_editContext.Field(nameof(_contactModel.ContactType)), $"There was a problem updating the contact information. Please try again but if this issue happens again then please report a bug.");
            _editContext.NotifyValidationStateChanged();
        }
    }

    private async Task<ContactModel?> GetContact()
    {
        var floodReport = await contactRepository.ReportedByUser(_userId, ContactId, _cts.Token).ConfigureAwait(false);
        if (floodReport == null || floodReport.ReportOwner == null)
        {
            return null;
        }
        return floodReport.ReportOwner.ToContactModel();
    }
}

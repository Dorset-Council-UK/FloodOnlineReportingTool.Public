using FloodOnlineReportingTool.Contracts.Shared;
using FloodOnlineReportingTool.Database.Models;
using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.Public.Models.FloodReport.Contact;
using FloodOnlineReportingTool.Public.Models.Order;
using FloodOnlineReportingTool.Public.Services;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport.Contacts;

public partial class Delete(
    ILogger<Delete> logger,
    NavigationManager navigationManager,
    SessionStateService scopedSessionStorage,
    IContactRecordRepository contactRepository,
    IGovNotifyEmailSender govNotifyEmailSender
) : IPageOrder, IAsyncDisposable
{
    // Public Properties
    public string Title { get; set; } = ContactPages.Delete.Title;
    public IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [
        GeneralPages.Home.ToGdsBreadcrumb(),
        FloodReportPages.Overview.ToGdsBreadcrumb(),
        ContactPages.Summary.ToGdsBreadcrumb(),
    ];

    // Parameters
    [Parameter]
    public Guid ContactId { get; set; }

    [CascadingParameter]
    public Task<AuthenticationState>? AuthenticationState { get; set; }

    // Private Fields
    private ContactModel? _contactModel;
    private EditContext _editContext = default!;
    private ValidationMessageStore _messageStore = default!;
    private Guid _floodReportId = Guid.Empty;
    private string _floodReportReference = string.Empty;
    private Guid _userId;
    private bool _isLoading = true;
    private bool _deletePermited = true;
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
            // Suppressing exception during disposal to prevent issues during component teardown
        }

        GC.SuppressFinalize(this);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _floodReportId = await scopedSessionStorage.GetFloodReportId();

            // Setup model and edit context
            if (_contactModel == null)
            {
                _contactModel = await GetContact();

                if (_contactModel != null)
                {
                    _editContext = new(_contactModel);
                    _messageStore = new(_editContext);
                }
            }

            _isLoading = false;
            StateHasChanged();

        }
    }

    private async Task OnValidSubmit()
    {
        logger.LogDebug("Deleting contact information");

        if (_contactModel?.Id == null || _contactModel?.ContactType == null)
        {
            return;
        }

        try
        {
            Guid contactRecordId = _contactModel.Id.Value;
            ContactRecordType contactRecordType = _contactModel.ContactType.Value;

            // Send deletion confirmation email just before deleting
            // TODO - enable this once notification is available
            //var sentNotification = await govNotifyEmailSender.SendContactDeletedNotification(_contactModel.EmailAddress!, _contactModel!.ContactName!, _floodReportReference, contactRecordType);

            var deleteResult = await contactRepository.DeleteById(contactRecordId, contactRecordType, _cts.Token);
            if (!deleteResult.IsSuccess)
            {
                var field = _editContext.Field(nameof(_contactModel.ContactType));
                foreach (var error in deleteResult.Errors)
                {
                    _messageStore.Add(field, error);
                }
                _editContext.NotifyValidationStateChanged();
                return;
            }

            // Navigate back to contacts home
            logger.LogInformation("Contact information deleted successfully for user");
            navigationManager.NavigateTo(ContactPages.Summary.Url);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "There was a problem deleting contact information");
            _messageStore.Add(
                _editContext.Field(nameof(_contactModel.ContactType)),
                $"There was a problem deleting the contact information. Please try again. If the problem continues, please contact support using the details on the Help or Contact us page and include your flood report reference '{_floodReportReference}'.");
            _editContext.NotifyValidationStateChanged();
        }
    }

    // Private Methods

    private async Task<ContactModel?> GetContact()
    {
        // Set safe defaults
        _deletePermited = false;
        _floodReportReference = string.Empty;

        var contactRecord = await contactRepository.GetContactById(ContactId, _cts.Token);
        if (contactRecord == null)
        {
            return null;
        }

        var floodReport = contactRecord.FloodReports.FirstOrDefault(fr => fr.Id == _floodReportId);
        if (contactRecord.FloodReports.Count == 1 && floodReport != null)
        {
            _deletePermited = true;
            _floodReportReference = floodReport.Reference;
        }

        return contactRecord.ToContactModel();
    }
}

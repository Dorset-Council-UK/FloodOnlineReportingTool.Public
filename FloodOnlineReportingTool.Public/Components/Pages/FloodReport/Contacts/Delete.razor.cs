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
    IGovNotifyEmailSender govNotifyEmailSender,
    IGdsJsInterop gdsJs
) : IPageOrder, IAsyncDisposable
{
    // Page order properties
    public string Title { get; set; } = ContactPages.Delete.Title;
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
    private Guid _floodReportId = Guid.Empty;
    private string _floodReportReference = string.Empty;
    private bool _isLoading = true;
    private bool _deletePermited = true;
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

    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await gdsJs.InitGds(_cts.Token);
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
            //Due to time taken to reach this we need to use the bigger push to ensure the page re-renders now
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task<ContactModel?> GetContact()
    {
        var contactResult = await contactRepository.GetContactById(ContactId, _cts.Token).ConfigureAwait(false);
        if (contactResult == null)
        {
            return null;
        }
        if (contactResult.FloodReports.Count > 1 || contactResult.FloodReports.Where(fr => fr.Id == _floodReportId).Count() == 0)
        {
            //You can only delete for the current flood report so we can't handle it for more one linked record
            _deletePermited = false;
        } else
        {
            //If we have a valid match then we return the reference for the current flood report only
            _floodReportReference = contactResult.FloodReports.FirstOrDefault(fr => fr.Id == _floodReportId)!.Reference;
        }

        return contactResult.ToContactModel();
    }

    private async Task OnValidSubmit()
    {
        logger.LogDebug("Deleting contact information");

        if (_contactModel == null)
        {
            return;
        }

        try
        {
            // Send deletion confirmation email just before deleting (fire and forget)
            _ = govNotifyEmailSender.SendContactDeletedNotification(_contactModel.EmailAddress, _contactModel!.ContactName, _floodReportReference, _contactModel.ContactType!.Value.ToString());

            await contactRepository.DeleteById(_contactModel.Id!.Value, _contactModel.ContactType!.Value, _cts.Token);
            logger.LogInformation("Contact information deleted successfully for user {UserId}", _userId);

            // Navigate back to contacts home
            navigationManager.NavigateTo(ContactPages.Home.Url);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "There was a problem deleting contact information");
            _messageStore.Add(_editContext.Field(nameof(_contactModel.ContactType)), $"There was a problem deleting the contact information. Please try again but if this issue happens again then please report a bug.");
            _editContext.NotifyValidationStateChanged();
        }
    }
}

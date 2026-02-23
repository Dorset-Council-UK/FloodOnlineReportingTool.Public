using FloodOnlineReportingTool.Contracts.Shared;
using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.Public.Models.FloodReport.Contact;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Identity.Web;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport.Contacts;

public partial class ContactInformation(IContactRecordRepository contactRepository) : IAsyncDisposable
{
    // Parameters
    [Parameter, EditorRequired]
    public required ContactModel Contact { get; set; }

    [Parameter]
    public required Guid FloodReportId { get; set; }

    [Parameter]
    public bool SummaryCard { get; set; } = false;

    [Parameter]
    public bool ViewOnly { get; set; } = false;

    [Parameter]
    public IReadOnlyCollection<GdsOptionItem<ContactRecordType>> ContactTypes { get; set; } = [];

    [CascadingParameter]
    public Task<AuthenticationState>? AuthenticationState { get; set; }

    // Private Fields
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

    protected override async Task OnInitializedAsync()
    {
        if (!SummaryCard && !ViewOnly)
        {
            if (ContactTypes.Count == 0)
            {
                ContactTypes = await CreateContactTypeOptions();
            }
        }
    }

    private async Task<IReadOnlyCollection<GdsOptionItem<ContactRecordType>>> CreateContactTypeOptions()
    {
        var userId = await GetUserIdAsGuid();
        if (userId == null)
        {
            return [];
        }

        IList<ContactRecordType> unusedRecordTypes = await contactRepository.GetUnusedRecordTypes(FloodReportId, _cts.Token);
        if (Contact.Id != null)
        {
            unusedRecordTypes.Add(Contact.ContactType.Value);
        }
        return [.. unusedRecordTypes.Select(CreateOption)];
    }

    /// <summary>
    /// Creates a GDS option item for a contact record type.
    /// </summary>
    private GdsOptionItem<ContactRecordType> CreateOption(ContactRecordType contactRecordType)
    {
        var id = contactRecordType.ToString().AsSpan();
        var selected = contactRecordType == Contact.ContactType;

        return new GdsOptionItem<ContactRecordType>(id, contactRecordType.LabelText(), contactRecordType, selected, hint: contactRecordType.HintText());
    }

    private async Task<string?> GetUserId()
    {
        if (AuthenticationState is null)
        {
            return null;
        }
        var authState = await AuthenticationState;
        return authState.User.GetObjectId();
    }

    private async Task<Guid?> GetUserIdAsGuid()
    {
        return Guid.TryParse(await GetUserId(), out var userId) ? userId : null;
    }
}

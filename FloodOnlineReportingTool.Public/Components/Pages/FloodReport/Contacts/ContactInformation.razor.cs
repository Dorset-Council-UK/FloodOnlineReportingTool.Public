using FloodOnlineReportingTool.Database.Models;
using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.Public.Models.FloodReport.Contact;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport.Contacts;

public partial class ContactInformation(IContactRecordRepository contactRepository, ProtectedSessionStorage protectedSessionStorage) : IAsyncDisposable
{
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
    private readonly CancellationTokenSource _cts = new();

    private IReadOnlyCollection<GdsOptionItem<bool>> _isPrimaryOptions = [
        new("is-primary-yes", "Yes", value: true),
        new("is-primary-no", "No", value: false),
    ];

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
        var userId = await AuthenticationState.IdentityUserId();
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

    private GdsOptionItem<ContactRecordType> CreateOption(ContactRecordType contactRecordType)
    {
        var id = contactRecordType.ToString().AsSpan();
        var label = contactRecordType is ContactRecordType.NonResident ? "Non resident".AsSpan() : id;
        var selected = contactRecordType == Contact!.ContactType;

        return new GdsOptionItem<ContactRecordType>(id, label, contactRecordType, selected);
    }

    private static string DescriptionText(ContactModel contactModel)
    {
        var contactType = contactModel.ContactType;
        if (contactType == null)
        {
            return "contact information";
        }

        return $"{contactType.Value.ToString().ToLowerInvariant()} contact information";
    }
}

using FloodOnlineReportingTool.DataAccess.Models;
using FloodOnlineReportingTool.DataAccess.Repositories;
using FloodOnlineReportingTool.GdsComponents;
using FloodOnlineReportingTool.Public.Models.FloodReport.Contact;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport.Contacts;

public partial class ContactInformation(IContactRecordRepository contactRepository) : IAsyncDisposable
{
    [Parameter, EditorRequired]
    public required ContactModel Contact { get; set; }

    [Parameter]
    public bool SummaryCard { get; set; } = false;
    
    [Parameter]
    public bool ViewOnly { get; set; } = false;

    [CascadingParameter]
    public Task<AuthenticationState>? AuthenticationState { get; set; }

    private IReadOnlyCollection<GdsOptionItem<ContactRecordType>> _contactTypes = [];
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
        if (!SummaryCard && !ViewOnly)
        {
            _contactTypes = await CreateContactTypeOptions();
        }
    }

    private async Task<IReadOnlyCollection<GdsOptionItem<ContactRecordType>>> CreateContactTypeOptions()
    {
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

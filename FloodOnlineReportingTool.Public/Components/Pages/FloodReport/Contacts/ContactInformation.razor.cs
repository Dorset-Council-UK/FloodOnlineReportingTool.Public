using FloodOnlineReportingTool.Contracts.Shared;
using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.Public.Models.FloodReport.Contact;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace FloodOnlineReportingTool.Public.Components.Pages.FloodReport.Contacts;

public partial class ContactInformation(IContactRecordRepository contactRepository) : IAsyncDisposable
{
    // Parameters
    [Parameter, EditorRequired]
    public required ContactModel Contact { get; set; }

    [Parameter]
    public required Guid FloodReportSourceId { get; set; }

    [Parameter]
    public bool SummaryCard { get; set; } = false;

    [Parameter]
    public bool ViewOnly { get; set; } = false;

    [Parameter]
    public IReadOnlyCollection<ContactRecordType> ContactTypes { get; set; } = [];

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

    private async Task<IReadOnlyCollection<ContactRecordType>> CreateContactTypeOptions()
    {
        IList<ContactRecordType> unusedRecordTypes = await contactRepository.GetUnusedRecordTypes(FloodReportSourceId, _cts.Token);
        if (Contact.Id != null)
        {
            unusedRecordTypes.Add(Contact.ContactType.Value);
        }
        return [.. unusedRecordTypes];
    }

}

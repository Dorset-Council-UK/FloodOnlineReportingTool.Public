using FloodOnlineReportingTool.Contracts.Shared;
using FloodOnlineReportingTool.Database.Models.Contact.Subscribe;
using FloodOnlineReportingTool.Database.Models.Flood;
using Microsoft.AspNetCore.Identity;

namespace FloodOnlineReportingTool.Database.Models.Contact;

/// <summary>
///     <para>Represents contact information for individuals reporting flood incidents and seeking assistance.</para>
///     <para>This can represent different types of contact records, for example tenant and home owner contact details.</para>
/// </summary>
/// <remarks>
///     <para>We are setting Oauth to be optional but if the Oid is set then the user details are coming from an account.</para>
/// </remarks>
public record ContactRecord
{
    public Guid Id { get; init; } = Guid.CreateVersion7();

    public DateTimeOffset CreatedUtc { get; init; }
    public DateTimeOffset? UpdatedUtc { get; init; }
    public DateTimeOffset RedactionDate { get; init; }
    public Guid? ContactUserId { get; set; }

    public ICollection<SubscribeRecord> SubscribeRecords { get; set; } = [];

    public ICollection<FloodReport> FloodReports { get; set; } = [];
}

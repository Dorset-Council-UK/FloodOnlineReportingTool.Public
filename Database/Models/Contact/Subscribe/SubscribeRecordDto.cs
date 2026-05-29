using FloodOnlineReportingTool.Contracts.Shared;

namespace FloodOnlineReportingTool.Database.Models.Contact.Subscribe;

/// <summary>
/// Defines the properties of subscribe record which can be updated
/// </summary>
public record SubscribeRecordDto
{
    public required ContactRecordType ContactType { get; init; }
    public required string ContactName { get; init; }
    public required string EmailAddress { get; init; }
    public bool IsRecordOwner { get; init; }
    public string? PhoneNumber { get; init; }
}

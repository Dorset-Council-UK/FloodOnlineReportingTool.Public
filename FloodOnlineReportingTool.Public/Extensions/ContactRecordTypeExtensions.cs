#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace FloodOnlineReportingTool.Contracts.Shared;
#pragma warning restore IDE0130 // Namespace does not match folder structure

internal static class ContactRecordTypeExtensions
{
    internal static string LabelText(this ContactRecordType? contactRecordType) => (contactRecordType ?? ContactRecordType.Unknown).LabelText();
    internal static string LabelText(this ContactRecordType contactRecordType) => contactRecordType switch
    {
        ContactRecordType.Tenant => "Tenant",
        ContactRecordType.HomeOwner => "Home owner",
        ContactRecordType.NonResident => "Non resident",
        _ => "Unknown",
    };

    internal static string HintText(this ContactRecordType? contactRecordType) => (contactRecordType ?? ContactRecordType.Unknown).HintText();
    internal static string HintText(this ContactRecordType contactRecordType) => contactRecordType switch
    {
        ContactRecordType.Tenant => "Tenant of the property affected by flooding.",
        ContactRecordType.HomeOwner => "Home owner of the property affected by flooding.",
        ContactRecordType.NonResident => "Does not live at or own the property affected by flooding.",
        _ => "",
    };
}

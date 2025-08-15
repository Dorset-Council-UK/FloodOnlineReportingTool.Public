namespace FloodOnlineReportingTool.Database.Models;

public readonly record struct EligibilityResult
(
    bool HasContactInformation,
    bool IsEmergencyResponse,
    string? Section19Url,
    EligibilityOptions Section19,
    EligibilityOptions FloodInvestigation,
    EligibilityOptions PropertyProtection,
    EligibilityOptions GrantApplication,
    IList<Organisation> ResponsibleOrganisations
);

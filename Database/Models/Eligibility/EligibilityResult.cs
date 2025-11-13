using FloodOnlineReportingTool.Database.Models.Responsibilities;

namespace FloodOnlineReportingTool.Database.Models.Eligibility;

public readonly record struct EligibilityResult
(
    bool HasContactInformation,
    bool IsEmergencyResponse,
    string? Section19Url,
    Guid FloodReportId,
    EligibilityOptions Section19,
    EligibilityOptions FloodInvestigation,
    EligibilityOptions PropertyProtection,
    EligibilityOptions GrantApplication,
    IList<Organisation> ResponsibleOrganisations
);

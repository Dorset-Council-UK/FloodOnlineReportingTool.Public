using FloodOnlineReportingTool.Contracts;
using FloodOnlineReportingTool.Database.DbContexts;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace FloodOnlineReportingTool.Database.Models;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public static class EligibilityCheckExtensions
{
    internal static EligibilityCheckCreated ToMessageCreated(this EligibilityCheck eligibilityCheck, string reference, IList<Organisation> organisations, IList<FloodProblem> floodProblems)
    {
        return new(
            eligibilityCheck.Id,
            reference,
            eligibilityCheck.CreatedUtc,
            eligibilityCheck.Uprn,
            eligibilityCheck.Usrn,
            eligibilityCheck.Easting,
            eligibilityCheck.Northing,
            eligibilityCheck.ImpactStart,
            eligibilityCheck.ImpactDuration,
            eligibilityCheck.OnGoing,
            eligibilityCheck.Uninhabitable,
            eligibilityCheck.VulnerableCount,
            [..
                organisations.Select(o => new EligibilityCheckOrganisation(
                    o.Id,
                    o.Name,
                    o.FloodAuthorityId,
                    o.FloodAuthority.AuthorityName
                )),
            ], [..
                floodProblems.Select(p => new EligibilityCheckFloodSource(
                    p.Id,
                    p.TypeName
                )),
            ]
        );
    }

    internal static EligibilityCheckUpdated ToMessageUpdated(this EligibilityCheck eligibilityCheck, IList<Organisation> organisations, IList<FloodProblem> floodProblems)
    {
        return new(
            eligibilityCheck.Id,
            eligibilityCheck.UpdatedUtc ?? DateTimeOffset.UtcNow,
            eligibilityCheck.Uprn,
            eligibilityCheck.Usrn,
            eligibilityCheck.Easting,
            eligibilityCheck.Northing,
            eligibilityCheck.ImpactStart,
            eligibilityCheck.ImpactDuration,
            eligibilityCheck.OnGoing,
            eligibilityCheck.Uninhabitable,
            eligibilityCheck.VulnerableCount,
            [..
                organisations.Select(o => new EligibilityCheckOrganisation(
                    o.Id,
                    o.Name,
                    o.FloodAuthorityId,
                    o.FloodAuthority.AuthorityName
                )),
            ], [..
                floodProblems.Select(p => new EligibilityCheckFloodSource(
                    p.Id,
                    p.TypeName
                )),
            ]
        );
    }

    internal static EligibilityCheckDto ToDto(this EligibilityCheck eligibilityCheck)
    {
        return new()
        {
            IsAddress = eligibilityCheck.IsAddress,
            Uprn = eligibilityCheck.Uprn,
            Usrn = eligibilityCheck.Usrn,
            Easting = eligibilityCheck.Easting,
            Northing = eligibilityCheck.Northing,
            LocationDesc = eligibilityCheck.LocationDesc,
            ImpactStart = eligibilityCheck.ImpactStart,
            ImpactDuration = eligibilityCheck.ImpactDuration,
            OnGoing = eligibilityCheck.OnGoing,
            Uninhabitable = eligibilityCheck.Uninhabitable,
            VulnerableCount = eligibilityCheck.VulnerableCount,
            Sources = [.. eligibilityCheck.Sources.Select(o => o.FloodProblemId)],
            Residentials = [.. eligibilityCheck.Residentials.Select(o => o.FloodImpactId)],
            Commercials = [.. eligibilityCheck.Commercials.Select(o => o.FloodImpactId)],
        };
    }

    /// <summary>
    /// Determines whether the eligibility check includes any internal flood impacts.
    /// </summary>
    /// <returns>
    /// True if any residential or commercial flood impacts have a category priority of internal; otherwise, false.
    /// </returns>
    public static bool IsInternal(this EligibilityCheck eligibilityCheck)
    {
        return
            eligibilityCheck.Residentials.Any(o => o.FloodImpact.IsInternal()) ||
            eligibilityCheck.Commercials.Any(o => o.FloodImpact.IsInternal());
    }

    /// <summary>
    /// Determines whether the eligibility check includes any external flood impacts.
    /// </summary>
    /// <returns>
    /// True if any residential or commercial flood impacts have a category priority of external; otherwise, false.
    /// </returns>
    public static bool IsExternal(this EligibilityCheck eligibilityCheck)
    {
        return
            eligibilityCheck.Residentials.Any(o => o.FloodImpact.IsExternal()) ||
            eligibilityCheck.Commercials.Any(o => o.FloodImpact.IsExternal());
    }

    // TODO: make a method for get score
    internal static int GetScore(this EligibilityCheck eligibilityCheck)
    {
        throw new NotImplementedException();
    }

    
}

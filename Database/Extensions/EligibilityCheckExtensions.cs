using FloodOnlineReportingTool.Contracts;

namespace FloodOnlineReportingTool.Database.Models;

public static class EligibilityCheckExtensions
{
    internal static EligibilityCheckCreated ToMessageCreated(this EligibilityCheck eligibilityCheck, string reference, IList<Organisation> organisations)
    {
        return new EligibilityCheckCreated(
            eligibilityCheck.Id,
            reference,
            eligibilityCheck.CreatedUtc,
            eligibilityCheck.Uprn,
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
            ]
        );
    }

    internal static EligibilityCheckUpdated ToMessageUpdated(this EligibilityCheck eligibilityCheck)
    {
        return new EligibilityCheckUpdated(
            eligibilityCheck.Id,
            eligibilityCheck.UpdatedUtc ?? DateTimeOffset.UtcNow,
            eligibilityCheck.Uprn,
            eligibilityCheck.Easting,
            eligibilityCheck.Northing,
            eligibilityCheck.ImpactStart,
            eligibilityCheck.ImpactDuration,
            eligibilityCheck.OnGoing,
            eligibilityCheck.Uninhabitable,
            eligibilityCheck.VulnerableCount
        );
    }

    public static EligibilityCheckDto ToDto(this EligibilityCheck eligibilityCheck)
    {
        return new EligibilityCheckDto
        {
            Uprn = eligibilityCheck.Uprn,
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
            eligibilityCheck.Residentials.Any(o => IsInternal(o.FloodImpact)) ||
            eligibilityCheck.Commercials.Any(o => IsInternal(o.FloodImpact));
    }

    private static bool IsInternal(FloodImpact floodImpact)
    {
        if (floodImpact.CategoryPriority is null)
        {
            return false;
        }

        return floodImpact.CategoryPriority.Equals(FloodImpactPriority.Internal, StringComparison.OrdinalIgnoreCase);
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
            eligibilityCheck.Residentials.Any(o => IsExternal(o.FloodImpact)) ||
            eligibilityCheck.Commercials.Any(o => IsExternal(o.FloodImpact));
    }

    private static bool IsExternal(FloodImpact floodImpact)
    {
        if (floodImpact.CategoryPriority is null)
        {
            return false;
        }

        return floodImpact.CategoryPriority.Equals(FloodImpactPriority.External, StringComparison.OrdinalIgnoreCase);
    }

    // TODO: make a method for get score
    public static int GetScore(this EligibilityCheck eligibilityCheck)
    {
        throw new NotImplementedException();
    }
}
